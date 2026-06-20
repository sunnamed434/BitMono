namespace BitMono.Core.Analyzing;

/// <summary>
/// Finds members that user code looks up by name through <see cref="System.Reflection"/> so the
/// renamer (and IL rewriters) leave them alone - renaming a reflected member turns a lookup into a
/// runtime <c>null</c>. Built on Washi's Echo data-flow graph (see <see cref="ReflectionDataFlow"/>):
/// the whole module is analyzed once, up front, and every query is then a pure identity lookup.
/// Intra-method only, like ConfuserEx. See ConfuserEx#39 for the original design discussion.
/// </summary>
[SuppressMessage("ReSharper", "InvertIf")]
public class ReflectionCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private enum MemberKind { Method, Field, Property, Event, NestedType, All }

    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly HashSet<ModuleDefinition> _analyzedModules = [];
    private readonly List<MethodDefinition> _cachedMethods = [];
    private readonly List<FieldDefinition> _cachedFields = [];
    private readonly List<PropertyDefinition> _cachedProperties = [];
    private readonly List<EventDefinition> _cachedEvents = [];
    private readonly List<TypeDefinition> _cachedTypes = [];
    private readonly List<MethodDefinition> _cachedMethodBodies = [];
    private ModuleDefinition? _analyzingModule;

    // Method names that may start a reflection lookup. Only a cheap pre-filter so we skip building a
    // data-flow graph for bodies that obviously can't reflect; the real check verifies declaring type.
    private static readonly HashSet<string> ReflectionMethodNames =
    [
        "GetMethod", "GetField", "GetProperty", "GetEvent", "GetMember", "InvokeMember",
        "GetNestedType", "GetInterface",
        "GetMethods", "GetFields", "GetProperties", "GetEvents", "GetMembers", "GetNestedTypes",
        "GetRuntimeMethod", "GetRuntimeField", "GetRuntimeProperty", "GetRuntimeEvent",
        "GetRuntimeMethods", "GetRuntimeFields", "GetRuntimeProperties", "GetRuntimeEvents",
        "GetType", "CreateInstance", "CreateInstanceFrom", "CreateDelegate",
        "GetName", "GetNames", "Parse", "TryParse", "IsDefined", "Format",
        "GetMethodBody", "GetILAsByteArray",
    ];

    public ReflectionCriticalAnalyzer(ObfuscationSettings obfuscationSettings)
    {
        _obfuscationSettings = obfuscationSettings;
    }

    public IReadOnlyList<MethodDefinition> CachedMethods => _cachedMethods.AsReadOnly();
    public IReadOnlyList<FieldDefinition> CachedFields => _cachedFields.AsReadOnly();
    public IReadOnlyList<PropertyDefinition> CachedProperties => _cachedProperties.AsReadOnly();
    public IReadOnlyList<EventDefinition> CachedEvents => _cachedEvents.AsReadOnly();
    public IReadOnlyList<TypeDefinition> CachedTypes => _cachedTypes.AsReadOnly();
    public IReadOnlyList<MethodDefinition> CachedMethodBodies => _cachedMethodBodies.AsReadOnly();

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        EnsureAnalyzed(ModuleOf(method));
        return !_cachedMethods.Contains(method);
    }

    public bool NotCriticalToMakeChanges(FieldDefinition field)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        EnsureAnalyzed(ModuleOf(field));
        return !_cachedFields.Contains(field);
    }

    public bool NotCriticalToMakeChanges(PropertyDefinition property)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        EnsureAnalyzed(ModuleOf(property));
        return !_cachedProperties.Contains(property);
    }

    public bool NotCriticalToMakeChanges(EventDefinition eventDef)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        EnsureAnalyzed(ModuleOf(eventDef));
        return !_cachedEvents.Contains(eventDef);
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }
        EnsureAnalyzed(ModuleOf(type));
        return !_cachedTypes.Contains(type);
    }

    /// <summary>
    /// Whether <paramref name="method"/>'s raw IL is read back via reflection and therefore must not
    /// be mutated by IL-rewriting protections. Renaming is still allowed (refetch-by-name works).
    /// </summary>
    public bool IsMethodBodyCritical(MethodDefinition method)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return false;
        }
        EnsureAnalyzed(ModuleOf(method));
        return _cachedMethodBodies.Contains(method);
    }

    private void EnsureAnalyzed(ModuleDefinition? module)
    {
        if (module == null || !_analyzedModules.Add(module))
        {
            return;
        }
        _analyzingModule = module;
        foreach (var type in module.GetAllTypes())
        {
            foreach (var method in type.Methods)
            {
                if (method.CilMethodBody is not { } body || !MayReflect(body))
                {
                    continue;
                }
                try
                {
                    AnalyzeBody(body, module);
                }
                catch
                {
                    // ponytail: skip a body Echo can't model rather than abort the whole-module pass;
                    // missing one obscure reflection site beats failing every member query.
                }
            }
        }
    }

    private void AnalyzeBody(CilMethodBody body, ModuleDefinition module)
    {
        body.ConstructSymbolicFlowGraph(out var dataFlowGraph);
        foreach (var node in dataFlowGraph.Nodes)
        {
            if (node.Contents is not { } instruction ||
                (instruction.OpCode.Code != CilCode.Call && instruction.OpCode.Code != CilCode.Callvirt))
            {
                continue;
            }
            var called = instruction.Operand as IMethodDefOrRef
                         ?? (instruction.Operand as MethodSpecification)?.Method;
            if (called != null)
            {
                AnalyzeCall(node, called, module);
            }
        }
    }

    private static bool MayReflect(CilMethodBody body)
    {
        foreach (var instruction in body.Instructions)
        {
            if (instruction.OpCode.Code is CilCode.Call or CilCode.Callvirt)
            {
                var name = (instruction.Operand as IMethodDefOrRef
                            ?? (instruction.Operand as MethodSpecification)?.Method)?.Name?.Value;
                if (name != null && ReflectionMethodNames.Contains(name))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void AnalyzeCall(DataFlowNode<CilInstruction> node, IMethodDefOrRef called, ModuleDefinition module)
    {
        var declaringType = called.DeclaringType;
        if (declaringType == null)
        {
            return;
        }
        var name = called.Name?.Value;
        if (name == null)
        {
            return;
        }

        if (declaringType.IsSystemType())
        {
            switch (name)
            {
                case "GetMethod": ExcludeMembers(node, module, MemberKind.Method); break;
                case "GetField": ExcludeMembers(node, module, MemberKind.Field); break;
                case "GetProperty": ExcludeMembers(node, module, MemberKind.Property); break;
                case "GetEvent": ExcludeMembers(node, module, MemberKind.Event); break;
                case "GetMember":
                case "InvokeMember": ExcludeMembers(node, module, MemberKind.All); break;
                case "GetNestedType": ExcludeMembers(node, module, MemberKind.NestedType); break;
                case "GetMethods": ExcludeAllMembers(node, module, MemberKind.Method); break;
                case "GetFields": ExcludeAllMembers(node, module, MemberKind.Field); break;
                case "GetProperties": ExcludeAllMembers(node, module, MemberKind.Property); break;
                case "GetEvents": ExcludeAllMembers(node, module, MemberKind.Event); break;
                case "GetMembers": ExcludeAllMembers(node, module, MemberKind.All); break;
                case "GetNestedTypes": ExcludeAllMembers(node, module, MemberKind.NestedType); break;
                case "GetType":
                case "GetInterface": ExcludeTypesByName(node, module); break;
            }
            return;
        }
        if (declaringType.IsTypeOf("System.Reflection", "RuntimeReflectionExtensions"))
        {
            switch (name)
            {
                case "GetRuntimeMethod": ExcludeMembers(node, module, MemberKind.Method); break;
                case "GetRuntimeField": ExcludeMembers(node, module, MemberKind.Field); break;
                case "GetRuntimeProperty": ExcludeMembers(node, module, MemberKind.Property); break;
                case "GetRuntimeEvent": ExcludeMembers(node, module, MemberKind.Event); break;
                case "GetRuntimeMethods": ExcludeAllMembers(node, module, MemberKind.Method); break;
                case "GetRuntimeFields": ExcludeAllMembers(node, module, MemberKind.Field); break;
                case "GetRuntimeProperties": ExcludeAllMembers(node, module, MemberKind.Property); break;
                case "GetRuntimeEvents": ExcludeAllMembers(node, module, MemberKind.Event); break;
            }
            return;
        }
        if (declaringType.IsTypeOf("System.Reflection", "Assembly") ||
            declaringType.IsTypeOf("System.Reflection", "Module"))
        {
            if (name == "GetType")
            {
                ExcludeTypesByName(node, module);
            }
            return;
        }
        if (declaringType.IsTypeOf("System", "Activator"))
        {
            if (name is "CreateInstance" or "CreateInstanceFrom")
            {
                ExcludeTypesByName(node, module);
            }
            return;
        }
        if (declaringType.IsTypeOf("System", "Delegate") || declaringType.IsTypeOf("System", "MulticastDelegate"))
        {
            if (name == "CreateDelegate")
            {
                ExcludeMembers(node, module, MemberKind.Method);
            }
            return;
        }
        if (declaringType.IsTypeOf("System", "Enum"))
        {
            if (name is "GetName" or "GetNames" or "Parse" or "TryParse" or "IsDefined" or "Format")
            {
                foreach (var type in ResolveTypes(node, module))
                {
                    if (type.IsEnum)
                    {
                        foreach (var field in type.Fields)
                        {
                            AddField(field);
                        }
                    }
                }
            }
            return;
        }
        if ((declaringType.IsTypeOf("System.Reflection", "MethodBody") && name == "GetILAsByteArray") ||
            (declaringType.IsTypeOf("System.Reflection", "MethodBase") && name == "GetMethodBody"))
        {
            ExcludeMethodBody(node, module);
        }
    }

    private void ExcludeMembers(DataFlowNode<CilInstruction> node, ModuleDefinition module, MemberKind kind)
    {
        var names = ResolveStrings(node);
        if (names.Count == 0)
        {
            return;
        }
        var types = ResolveTypes(node, module);
        // Known target type -> precise (that type + its bases). Unknown (e.g. instance.GetType()) ->
        // fall back to a name-scoped sweep of the module. Never widens past the queried name.
        var searchTypes = types.Count > 0
            ? types.SelectMany(t => t.GetTypeAndBaseTypes())
            : module.GetAllTypes();
        foreach (var type in searchTypes)
        {
            foreach (var name in names)
            {
                ExcludeMatchingMembers(type, name, kind);
            }
        }
    }

    private void ExcludeAllMembers(DataFlowNode<CilInstruction> node, ModuleDefinition module, MemberKind kind)
    {
        var types = ResolveTypes(node, module);
        if (types.Count == 0)
        {
            // GetMethods()/GetFields()/... with an unknown type: refuse to freeze every member of a
            // kind across the module. Caller must pin the type for a plural enumeration to count.
            return;
        }
        foreach (var type in types.SelectMany(t => t.GetTypeAndBaseTypes()))
        {
            switch (kind)
            {
                case MemberKind.Method: foreach (var m in type.Methods) AddMethod(m); break;
                case MemberKind.Field: foreach (var f in type.Fields) AddField(f); break;
                case MemberKind.Property: foreach (var p in type.Properties) AddProperty(p); break;
                case MemberKind.Event: foreach (var e in type.Events) AddEvent(e); break;
                case MemberKind.NestedType: foreach (var n in type.NestedTypes) AddType(n); break;
                case MemberKind.All:
                    foreach (var m in type.Methods) AddMethod(m);
                    foreach (var f in type.Fields) AddField(f);
                    foreach (var p in type.Properties) AddProperty(p);
                    foreach (var e in type.Events) AddEvent(e);
                    foreach (var n in type.NestedTypes) AddType(n);
                    break;
            }
        }
    }

    private void ExcludeMatchingMembers(TypeDefinition type, string name, MemberKind kind)
    {
        if (kind is MemberKind.Method or MemberKind.All)
        {
            foreach (var method in type.Methods)
            {
                if (method.Name == name) AddMethod(method);
            }
        }
        if (kind is MemberKind.Field or MemberKind.All)
        {
            foreach (var field in type.Fields)
            {
                if (field.Name == name) AddField(field);
            }
        }
        if (kind is MemberKind.Property or MemberKind.All)
        {
            foreach (var property in type.Properties)
            {
                if (property.Name == name) AddProperty(property);
            }
        }
        if (kind is MemberKind.Event or MemberKind.All)
        {
            foreach (var eventDef in type.Events)
            {
                if (eventDef.Name == name) AddEvent(eventDef);
            }
        }
        if (kind is MemberKind.NestedType or MemberKind.All)
        {
            foreach (var nested in type.NestedTypes)
            {
                if (nested.Name == name) AddType(nested);
            }
        }
    }

    private void ExcludeTypesByName(DataFlowNode<CilInstruction> node, ModuleDefinition module)
    {
        foreach (var name in ResolveStrings(node))
        {
            var type = ResolveTypeByName(module, name);
            if (type != null)
            {
                AddType(type);
            }
        }
    }

    private void ExcludeMethodBody(DataFlowNode<CilInstruction> node, ModuleDefinition module)
    {
        // Best-effort: walk back from GetILAsByteArray()/GetMethodBody() through the receiver chain to
        // the GetMethod(...) that produced the MethodInfo, then mark those targets. If the MethodInfo
        // source can't be pinned we simply can't tell which method's IL is read (same limit ConfuserEx
        // documented) - rename exclusion via the normal path still applies.
        var visited = new HashSet<DataFlowNode<CilInstruction>>();
        var queue = new Queue<DataFlowNode<CilInstruction>>(ReflectionDataFlow.OperandNodes(node));
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current) || current.Contents is not { } instruction)
            {
                continue;
            }
            var called = instruction.Operand as IMethodDefOrRef
                         ?? (instruction.Operand as MethodSpecification)?.Method;
            if (called?.DeclaringType?.IsSystemType() == true && called.Name?.Value is "GetMethod" or "GetMethods")
            {
                var names = ResolveStrings(current);
                var types = ResolveTypes(current, module);
                var searchTypes = types.Count > 0
                    ? types.SelectMany(t => t.GetTypeAndBaseTypes())
                    : module.GetAllTypes();
                foreach (var type in searchTypes)
                {
                    foreach (var method in type.Methods)
                    {
                        if ((names.Count == 0 || names.Contains(method.Name?.Value)) && InModule(method) &&
                            !_cachedMethodBodies.Contains(method))
                        {
                            _cachedMethodBodies.Add(method);
                        }
                    }
                }
            }
            else
            {
                // chain through e.g. MethodBase.GetMethodBody()
                foreach (var operand in ReflectionDataFlow.OperandNodes(current))
                {
                    queue.Enqueue(operand);
                }
            }
        }
    }

    private List<string> ResolveStrings(DataFlowNode<CilInstruction> node)
    {
        var result = new List<string>();
        foreach (var operand in ReflectionDataFlow.OperandNodes(node))
        {
            if (operand.Contents?.OpCode.Code == CilCode.Ldstr && operand.Contents.Operand is string value &&
                !result.Contains(value))
            {
                result.Add(value);
            }
        }
        return result;
    }

    private List<TypeDefinition> ResolveTypes(DataFlowNode<CilInstruction> node, ModuleDefinition module)
    {
        var result = new List<TypeDefinition>();
        void Add(TypeDefinition? type)
        {
            if (type != null && !result.Contains(type))
            {
                result.Add(type);
            }
        }

        foreach (var operand in ReflectionDataFlow.OperandNodes(node))
        {
            Add(ResolveTypeOperand(operand, module));
        }
        // Generic reflection (e.g. Enum.Parse<TEnum>): the type rides in the method's type arguments.
        if (node.Contents?.Operand is MethodSpecification { Signature: { } signature })
        {
            foreach (var argument in signature.TypeArguments)
            {
                Add(argument.ToTypeDefOrRef().ResolveOrNull());
            }
        }
        return result;
    }

    private TypeDefinition? ResolveTypeOperand(DataFlowNode<CilInstruction> node, ModuleDefinition module)
    {
        var instruction = node.Contents;
        if (instruction == null)
        {
            return null;
        }
        if (instruction.OpCode.Code == CilCode.Ldtoken && instruction.Operand is ITypeDefOrRef typeRef)
        {
            return typeRef.ResolveOrNull();
        }
        if (instruction.OpCode.Code is CilCode.Call or CilCode.Callvirt &&
            instruction.Operand is IMethodDefOrRef called && called.DeclaringType is { } declaringType)
        {
            var name = called.Name?.Value;
            if (declaringType.IsSystemType() && name == "GetTypeFromHandle")
            {
                foreach (var operand in ReflectionDataFlow.OperandNodes(node))
                {
                    if (operand.Contents?.OpCode.Code == CilCode.Ldtoken &&
                        operand.Contents.Operand is ITypeDefOrRef handleType)
                    {
                        return handleType.ResolveOrNull();
                    }
                }
            }
            if (name == "GetType" && (declaringType.IsSystemType() ||
                declaringType.IsTypeOf("System.Reflection", "Assembly") ||
                declaringType.IsTypeOf("System.Reflection", "Module")))
            {
                foreach (var operand in ReflectionDataFlow.OperandNodes(node))
                {
                    if (operand.Contents?.OpCode.Code == CilCode.Ldstr && operand.Contents.Operand is string typeName)
                    {
                        var resolved = ResolveTypeByName(module, typeName);
                        if (resolved != null)
                        {
                            return resolved;
                        }
                    }
                }
            }
        }
        return null;
    }

    private static TypeDefinition? ResolveTypeByName(ModuleDefinition module, string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        // Drop any assembly-qualified suffix: "Ns.Type, Assembly" -> "Ns.Type".
        var comma = name!.IndexOf(',');
        if (comma >= 0)
        {
            name = name.Substring(0, comma).Trim();
        }
        return module.GetAllTypes().FirstOrDefault(t => t.FullName == name);
    }

    private void AddMethod(MethodDefinition method)
    {
        if (!InModule(method) || _cachedMethods.Contains(method))
        {
            return;
        }
        _cachedMethods.Add(method);
        // A reflected override implies its base declarations are reflected too - freeze them so the
        // inheritance contract survives renaming.
        if (method.DeclaringType?.BaseType?.ResolveOrNull() is { } baseType)
        {
            foreach (var baseMethod in baseType.Methods)
            {
                if (IsMethodOverride(method, baseMethod))
                {
                    AddMethod(baseMethod);
                }
            }
        }
    }

    private void AddField(FieldDefinition field)
    {
        if (InModule(field) && !_cachedFields.Contains(field))
        {
            _cachedFields.Add(field);
        }
    }

    private void AddProperty(PropertyDefinition property)
    {
        if (!InModule(property) || _cachedProperties.Contains(property))
        {
            return;
        }
        _cachedProperties.Add(property);
        if (property.DeclaringType?.BaseType?.ResolveOrNull() is { } baseType)
        {
            foreach (var baseProperty in baseType.Properties)
            {
                if (baseProperty.Name == property.Name)
                {
                    AddProperty(baseProperty);
                }
            }
        }
    }

    private void AddEvent(EventDefinition eventDef)
    {
        if (!InModule(eventDef) || _cachedEvents.Contains(eventDef))
        {
            return;
        }
        _cachedEvents.Add(eventDef);
        if (eventDef.DeclaringType?.BaseType?.ResolveOrNull() is { } baseType)
        {
            foreach (var baseEvent in baseType.Events)
            {
                if (baseEvent.Name == eventDef.Name)
                {
                    AddEvent(baseEvent);
                }
            }
        }
    }

    private void AddType(TypeDefinition type)
    {
        if (InModule(type) && !_cachedTypes.Contains(type))
        {
            _cachedTypes.Add(type);
        }
    }

    private bool InModule(IMetadataMember member) => ModuleOf(member) == _analyzingModule;

    private static ModuleDefinition? ModuleOf(IMetadataMember member) => member switch
    {
        TypeDefinition type => type.DeclaringModule,
        IMemberDefinition definition => definition.DeclaringType?.DeclaringModule,
        _ => null
    };

    private static bool IsMethodOverride(MethodDefinition derived, MethodDefinition baseMethod)
    {
        if (derived.Name != baseMethod.Name ||
            derived.Signature == null || baseMethod.Signature == null ||
            derived.Signature.ParameterTypes.Count != baseMethod.Signature.ParameterTypes.Count ||
            !derived.Signature.ReturnType.Equals(baseMethod.Signature.ReturnType))
        {
            return false;
        }
        for (var i = 0; i < derived.Signature.ParameterTypes.Count; i++)
        {
            if (!derived.Signature.ParameterTypes[i].Equals(baseMethod.Signature.ParameterTypes[i]))
            {
                return false;
            }
        }
        return true;
    }
}
