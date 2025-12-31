namespace BitMono.Core.Analyzing;

[SuppressMessage("ReSharper", "InvertIf")]
public class ReflectionCriticalAnalyzer : ICriticalAnalyzer<MethodDefinition>
{
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly ArgumentTracer _argumentTracer;
    private readonly List<MethodDefinition> _cachedMethods;
    private readonly List<FieldDefinition> _cachedFields;
    private readonly List<PropertyDefinition> _cachedProperties;
    private readonly List<EventDefinition> _cachedEvents;
    private readonly List<TypeDefinition> _cachedTypes;

    private static readonly string[] ReflectionMethods =
    [
        nameof(Type.GetMethod),
        nameof(Type.GetField),
        nameof(Type.GetProperty),
        nameof(Type.GetEvent),
        nameof(Type.GetMember),
        nameof(Type.GetTypeFromHandle),
        nameof(Type.GetMethods),
        nameof(Type.GetFields),
        nameof(Type.GetProperties),
        nameof(Type.GetEvents),
        nameof(Type.GetMembers)
    ];

    private enum MemberType
    {
        Method,
        Field,
        Property,
        Event,
        All
    }

    public ReflectionCriticalAnalyzer(ObfuscationSettings obfuscationSettings)
    {
        _obfuscationSettings = obfuscationSettings;
        _argumentTracer = new ArgumentTracer();
        _cachedMethods = [];
        _cachedFields = [];
        _cachedProperties = [];
        _cachedEvents = [];
        _cachedTypes = [];
    }

    public IReadOnlyList<MethodDefinition> CachedMethods => _cachedMethods.AsReadOnly();
    public IReadOnlyList<FieldDefinition> CachedFields => _cachedFields.AsReadOnly();
    public IReadOnlyList<PropertyDefinition> CachedProperties => _cachedProperties.AsReadOnly();
    public IReadOnlyList<EventDefinition> CachedEvents => _cachedEvents.AsReadOnly();
    public IReadOnlyList<TypeDefinition> CachedTypes => _cachedTypes.AsReadOnly();

    public bool NotCriticalToMakeChanges(MethodDefinition method)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }

        if (_cachedMethods.FirstOrDefault(x => x.Name.Equals(method.Name)) != null)
        {
            return false;
        }

        bool foundReflection = false;

        if (method.CilMethodBody is { } body)
        {
            body.ConstructSymbolicFlowGraph(out var dataFlowGraph);
            foreach (var node in dataFlowGraph.Nodes)
            {
                var orderedDependencies =
                    node.GetOrderedDependencies(DependencyCollectionFlags.IncludeStackDependencies);
                foreach (var order in orderedDependencies)
                {
                    var instruction = order.Contents;
                    if ((instruction?.OpCode.Code == CilCode.Call || instruction?.OpCode.Code == CilCode.Callvirt) &&
                        instruction.Operand is IMethodDefOrRef calledMethod)
                    {
                        if (calledMethod.DeclaringType.IsSystemType() &&
                            ReflectionMethods.Contains(calledMethod.Name.Value))
                        {
                            foundReflection |=
                                AnalyzeReflectionCall(body, instruction, calledMethod, method.DeclaringModule);
                        }
                        else if (calledMethod.DeclaringType.IsSystemType() &&
                                 calledMethod.Name.Value == nameof(Type.GetTypeFromHandle))
                        {
                            var typeFromHandle = _argumentTracer.TraceTypeArgument(body, instruction, 0);
                            if (typeFromHandle != null && !_cachedTypes.Contains(typeFromHandle))
                            {
                                _cachedTypes.Add(typeFromHandle);
                                foundReflection = true;
                            }
                        }
                    }
                    else if (instruction?.OpCode == CilOpCodes.Ldtoken && instruction.Operand is ITypeDefOrRef typeRef)
                    {
                        if (typeRef.Resolve() is TypeDefinition typeDef && !_cachedTypes.Contains(typeDef))
                        {
                            _cachedTypes.Add(typeDef);
                            foundReflection = true;
                        }
                    }
                }
            }
        }

        if (foundReflection && !_cachedMethods.Contains(method))
        {
            _cachedMethods.Add(method);
        }

        return !foundReflection;
    }

    public bool NotCriticalToMakeChanges(FieldDefinition field)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }

        return _cachedFields.FirstOrDefault(x => x.Name.Equals(field.Name)) == null;
    }

    public bool NotCriticalToMakeChanges(PropertyDefinition property)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }

        return _cachedProperties.FirstOrDefault(x => x.Name.Equals(property.Name)) == null;
    }

    public bool NotCriticalToMakeChanges(EventDefinition eventDef)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }

        return _cachedEvents.FirstOrDefault(x => x.Name.Equals(eventDef.Name)) == null;
    }

    public bool NotCriticalToMakeChanges(TypeDefinition type)
    {
        if (!_obfuscationSettings.ReflectionMembersObfuscationExclude)
        {
            return true;
        }

        return _cachedTypes.FirstOrDefault(x => x.Name.Equals(type.Name)) == null;
    }

    private bool AnalyzeReflectionCall(CilMethodBody body, CilInstruction instruction, IMethodDefOrRef calledMethod,
        ModuleDefinition module)
    {
        var argumentIndices = _argumentTracer.TraceArguments(body, instruction);
        if (argumentIndices == null)
        {
            return AnalyzeReflectionCallLegacy(body, instruction, calledMethod, module);
        }

        TypeDefinition? targetType = null;
        if (instruction.Operand is IMethodDefOrRef methodRef)
        {
            var resolvedMethod = methodRef.Resolve();
            if (resolvedMethod?.Signature != null && resolvedMethod.Signature.HasThis)
            {
                var argumentCount = resolvedMethod.Signature.ParameterTypes.Count;
                if (resolvedMethod.Signature.HasThis)
                {
                    argumentCount++;
                }

                var thisArgumentIndices = _argumentTracer.TraceArgumentsRobust(body, instruction, argumentCount);
                if (thisArgumentIndices != null && thisArgumentIndices.Length > 0)
                {
                    targetType = ExtractTargetType(body, thisArgumentIndices, thisArgumentIndices.Length - 1);
                }
            }
        }

        var typesToSearch = targetType != null
            ? targetType.GetTypeAndBaseTypes()
            : module.GetAllTypes();

        var methodName = calledMethod.Name.Value;
        bool foundReflection = false;

        var memberName = _argumentTracer.TraceStringArgument(body, instruction, 0);

        switch (methodName)
        {
            case nameof(Type.GetMethod):
                foundReflection |= AnalyzeSingleMember(body, instruction, typesToSearch, memberName, MemberType.Method);
                break;
            case nameof(Type.GetField):
                foundReflection |= AnalyzeSingleMember(body, instruction, typesToSearch, memberName, MemberType.Field);
                break;
            case nameof(Type.GetProperty):
                foundReflection |=
                    AnalyzeSingleMember(body, instruction, typesToSearch, memberName, MemberType.Property);
                break;
            case nameof(Type.GetEvent):
                foundReflection |= AnalyzeSingleMember(body, instruction, typesToSearch, memberName, MemberType.Event);
                break;
            case nameof(Type.GetMember):
                foundReflection |= AnalyzeAllMembers(body, instruction, typesToSearch, memberName);
                break;
            case nameof(Type.GetMethods):
                foundReflection |= AnalyzeAllMembers(typesToSearch, MemberType.Method);
                break;
            case nameof(Type.GetFields):
                foundReflection |= AnalyzeAllMembers(typesToSearch, MemberType.Field);
                break;
            case nameof(Type.GetProperties):
                foundReflection |= AnalyzeAllMembers(typesToSearch, MemberType.Property);
                break;
            case nameof(Type.GetEvents):
                foundReflection |= AnalyzeAllMembers(typesToSearch, MemberType.Event);
                break;
            case nameof(Type.GetMembers):
                foundReflection |= AnalyzeAllMembers(typesToSearch, MemberType.All);
                break;
        }

        return foundReflection;
    }

    private TypeDefinition? ExtractTargetType(CilMethodBody body, int[] argumentIndices, int typeArgumentIndex)
    {
        if (typeArgumentIndex >= argumentIndices.Length)
            return null;

        var typeInstructionIndex = argumentIndices[typeArgumentIndex];
        if (typeInstructionIndex < 0 || typeInstructionIndex >= body.Instructions.Count)
            return null;

        var typeInstruction = body.Instructions[typeInstructionIndex];

        if (typeInstruction.OpCode == CilOpCodes.Ldtoken && typeInstruction.Operand is ITypeDefOrRef typeRef)
        {
            return typeRef.Resolve();
        }

        if (typeInstruction.OpCode == CilOpCodes.Call && typeInstruction.Operand is IMethodDefOrRef method)
        {
            if (method.DeclaringType.IsSystemType() && method.Name.Value == nameof(Type.GetTypeFromHandle))
            {
                return _argumentTracer.TraceTypeArgument(body, typeInstruction, 0);
            }
        }

        return null;
    }

    private bool AnalyzeSingleMember(CilMethodBody body, CilInstruction callInstruction,
        IEnumerable<TypeDefinition> typesToSearch, string? memberName, MemberType memberType)
    {
        if (string.IsNullOrEmpty(memberName))
            return false;

        bool found = false;
        foreach (var type in typesToSearch)
        {
            switch (memberType)
            {
                case MemberType.Method:
                    foreach (var method in type.Methods.Where(m => m.Name.Equals(memberName)))
                    {
                        if (!_cachedMethods.Contains(method))
                        {
                            _cachedMethods.Add(method);
                            found = true;
                        }

                        HandleMemberOverrideReferences(method);
                    }

                    break;
                case MemberType.Field:
                    foreach (var field in type.Fields.Where(f => f.Name.Equals(memberName)))
                    {
                        if (!_cachedFields.Contains(field))
                        {
                            _cachedFields.Add(field);
                            found = true;
                        }
                    }

                    break;
                case MemberType.Property:
                    foreach (var property in type.Properties.Where(p => p.Name.Equals(memberName)))
                    {
                        if (!_cachedProperties.Contains(property))
                        {
                            _cachedProperties.Add(property);
                            found = true;
                        }

                        HandleMemberOverrideReferences(property);
                    }

                    break;
                case MemberType.Event:
                    foreach (var eventDef in type.Events.Where(e => e.Name.Equals(memberName)))
                    {
                        if (!_cachedEvents.Contains(eventDef))
                        {
                            _cachedEvents.Add(eventDef);
                            found = true;
                        }

                        HandleMemberOverrideReferences(eventDef);
                    }

                    break;
            }
        }

        return found;
    }

    private bool AnalyzeAllMembers(CilMethodBody body, CilInstruction callInstruction,
        IEnumerable<TypeDefinition> typesToSearch, string? memberName)
    {
        if (string.IsNullOrEmpty(memberName))
            return false;

        bool found = false;
        foreach (var type in typesToSearch)
        {
            foreach (var method in type.Methods.Where(m => m.Name.Equals(memberName)))
            {
                if (!_cachedMethods.Contains(method))
                {
                    _cachedMethods.Add(method);
                    found = true;
                }
            }

            foreach (var field in type.Fields.Where(f => f.Name.Equals(memberName)))
            {
                if (!_cachedFields.Contains(field))
                {
                    _cachedFields.Add(field);
                    found = true;
                }
            }

            foreach (var property in type.Properties.Where(p => p.Name.Equals(memberName)))
            {
                if (!_cachedProperties.Contains(property))
                {
                    _cachedProperties.Add(property);
                    found = true;
                }
            }

            foreach (var eventDef in type.Events.Where(e => e.Name.Equals(memberName)))
            {
                if (!_cachedEvents.Contains(eventDef))
                {
                    _cachedEvents.Add(eventDef);
                    found = true;
                }
            }

            if (type.Name.Equals(memberName) && !_cachedTypes.Contains(type))
            {
                _cachedTypes.Add(type);
                found = true;
            }
        }

        return found;
    }

    private bool AnalyzeAllMembers(IEnumerable<TypeDefinition> typesToSearch, MemberType memberType)
    {
        bool found = false;
        foreach (var type in typesToSearch)
        {
            switch (memberType)
            {
                case MemberType.Method:
                    foreach (var method in type.Methods)
                    {
                        if (!_cachedMethods.Contains(method))
                        {
                            _cachedMethods.Add(method);
                            found = true;
                        }
                    }
                    break;
                case MemberType.Field:
                    foreach (var field in type.Fields)
                    {
                        if (!_cachedFields.Contains(field))
                        {
                            _cachedFields.Add(field);
                            found = true;
                        }
                    }
                    break;
                case MemberType.Property:
                    foreach (var property in type.Properties)
                    {
                        if (!_cachedProperties.Contains(property))
                        {
                            _cachedProperties.Add(property);
                            found = true;
                        }
                    }
                    break;
                case MemberType.Event:
                    foreach (var eventDef in type.Events)
                    {
                        if (!_cachedEvents.Contains(eventDef))
                        {
                            _cachedEvents.Add(eventDef);
                            found = true;
                        }
                    }
                    break;
                case MemberType.All:
                    foreach (var method in type.Methods)
                    {
                        if (!_cachedMethods.Contains(method))
                        {
                            _cachedMethods.Add(method);
                            found = true;
                        }
                    }

                    foreach (var field in type.Fields)
                    {
                        if (!_cachedFields.Contains(field))
                        {
                            _cachedFields.Add(field);
                            found = true;
                        }
                    }

                    foreach (var property in type.Properties)
                    {
                        if (!_cachedProperties.Contains(property))
                        {
                            _cachedProperties.Add(property);
                            found = true;
                        }
                    }

                    foreach (var eventDef in type.Events)
                    {
                        if (!_cachedEvents.Contains(eventDef))
                        {
                            _cachedEvents.Add(eventDef);
                            found = true;
                        }
                    }

                    break;
            }
        }

        return found;
    }

    private bool AnalyzeReflectionCallLegacy(CilMethodBody body, CilInstruction instruction,
        IMethodDefOrRef calledMethod, ModuleDefinition module)
    {
        var traceArgument = TraceStringArgument(body, instruction);
        if (traceArgument?.Operand is string memberName)
        {
            var allMembers = module.FindMembers();
            return ProcessLegacyReflection(calledMethod, memberName, allMembers);
        }

        return false;
    }

    private bool ProcessLegacyReflection(IMethodDefOrRef calledMethod, string memberName,
        List<IMetadataMember> allMembers)
    {
        bool found = false;
        switch (calledMethod.Name.Value)
        {
            case nameof(Type.GetMethod):
                foreach (var possibleMethod in allMembers.OfType<MethodDefinition>()
                             .Where(x => x.Name.Equals(memberName)))
                {
                    if (!_cachedMethods.Contains(possibleMethod))
                    {
                        _cachedMethods.Add(possibleMethod);
                        found = true;
                    }
                }

                break;
            case nameof(Type.GetField):
                foreach (var possibleField in allMembers.OfType<FieldDefinition>()
                             .Where(x => x.Name.Equals(memberName)))
                {
                    if (!_cachedFields.Contains(possibleField))
                    {
                        _cachedFields.Add(possibleField);
                        found = true;
                    }
                }

                break;
            case nameof(Type.GetProperty):
                foreach (var possibleProperty in allMembers.OfType<PropertyDefinition>()
                             .Where(x => x.Name.Equals(memberName)))
                {
                    if (!_cachedProperties.Contains(possibleProperty))
                    {
                        _cachedProperties.Add(possibleProperty);
                        found = true;
                    }
                }

                break;
            case nameof(Type.GetEvent):
                foreach (var possibleEvent in allMembers.OfType<EventDefinition>()
                             .Where(x => x.Name.Equals(memberName)))
                {
                    if (!_cachedEvents.Contains(possibleEvent))
                    {
                        _cachedEvents.Add(possibleEvent);
                        found = true;
                    }
                }

                break;
            case nameof(Type.GetMember):
                foreach (var possibleMember in allMembers)
                {
                    string? memberNameToCheck = possibleMember switch
                    {
                        MethodDefinition m => m.Name,
                        FieldDefinition f => f.Name,
                        PropertyDefinition p => p.Name,
                        EventDefinition e => e.Name,
                        TypeDefinition t => t.Name,
                        _ => null
                    };

                    if (memberNameToCheck != null && memberNameToCheck.Equals(memberName))
                    {
                        switch (possibleMember)
                        {
                            case MethodDefinition m:
                                if (!_cachedMethods.Contains(m))
                                {
                                    _cachedMethods.Add(m);
                                }

                                break;
                            case FieldDefinition f:
                                if (!_cachedFields.Contains(f))
                                {
                                    _cachedFields.Add(f);
                                }

                                break;
                            case PropertyDefinition p:
                                if (!_cachedProperties.Contains(p))
                                {
                                    _cachedProperties.Add(p);
                                }

                                break;
                            case EventDefinition e:
                                if (!_cachedEvents.Contains(e))
                                {
                                    _cachedEvents.Add(e);
                                }

                                break;
                            case TypeDefinition t:
                                if (!_cachedTypes.Contains(t))
                                {
                                    _cachedTypes.Add(t);
                                }

                                break;
                        }

                        found = true;
                    }
                }
                break;
        }
        return found;
    }

    private static CilInstruction? TraceStringArgument(CilMethodBody body, CilInstruction instruction)
    {
        var callIndex = body.Instructions.IndexOf(instruction);
        if (callIndex <= 0) return null;

        for (var i = callIndex - 1; i >= 0; i--)
        {
            var previousInstruction = body.Instructions[i];

            if (previousInstruction.OpCode == CilOpCodes.Ldstr)
            {
                return previousInstruction;
            }

            if (previousInstruction.OpCode == CilOpCodes.Ldloc_0 ||
                previousInstruction.OpCode == CilOpCodes.Ldloc_1 ||
                previousInstruction.OpCode == CilOpCodes.Ldloc_2 ||
                previousInstruction.OpCode == CilOpCodes.Ldloc_3 ||
                previousInstruction.OpCode == CilOpCodes.Ldloc_S ||
                previousInstruction.OpCode == CilOpCodes.Ldloc)
            {
                var variableIndex = GetVariableIndex(previousInstruction);
                if (variableIndex >= 0)
                {
                    var stringInstruction = FindStringAssignmentToVariable(body, variableIndex, i);
                    if (stringInstruction != null)
                    {
                        return stringInstruction;
                    }
                }
            }
        }

        return null;
    }

    private static int GetVariableIndex(CilInstruction instruction)
    {
        return instruction.OpCode.Code switch
        {
            CilCode.Ldloc_0 => 0,
            CilCode.Ldloc_1 => 1,
            CilCode.Ldloc_2 => 2,
            CilCode.Ldloc_3 => 3,
            CilCode.Ldloc_S => ((CilLocalVariable)instruction.Operand!).Index,
            CilCode.Ldloc => ((CilLocalVariable)instruction.Operand!).Index,
            _ => -1
        };
    }

    private static CilInstruction? FindStringAssignmentToVariable(CilMethodBody body, int variableIndex, int maxIndex)
    {
        for (var i = 0; i < maxIndex; i++)
        {
            var instruction = body.Instructions[i];

            if ((instruction.OpCode == CilOpCodes.Stloc_0 && variableIndex == 0) ||
                (instruction.OpCode == CilOpCodes.Stloc_1 && variableIndex == 1) ||
                (instruction.OpCode == CilOpCodes.Stloc_2 && variableIndex == 2) ||
                (instruction.OpCode == CilOpCodes.Stloc_3 && variableIndex == 3) ||
                (instruction.OpCode == CilOpCodes.Stloc_S && ((CilLocalVariable)instruction.Operand!).Index == variableIndex) ||
                (instruction.OpCode == CilOpCodes.Stloc && ((CilLocalVariable)instruction.Operand!).Index == variableIndex))
            {
                for (var j = i - 1; j >= 0; j--)
                {
                    var prevInstruction = body.Instructions[j];
                    if (prevInstruction.OpCode == CilOpCodes.Ldstr)
                    {
                        return prevInstruction;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Handles member override references to prevent base members from being obfuscated.
    /// </summary>
    /// <param name="member">The member that was found through reflection.</param>
    private void HandleMemberOverrideReferences(IMemberDefinition member)
    {
        if (member is not (MethodDefinition or PropertyDefinition or EventDefinition))
            return;

        var baseMembers = GetBaseMembers(member);

        foreach (var baseMember in baseMembers)
        {
            switch (baseMember)
            {
                case MethodDefinition baseMethod when !_cachedMethods.Contains(baseMethod):
                    _cachedMethods.Add(baseMethod);
                    break;
                case PropertyDefinition baseProperty when !_cachedProperties.Contains(baseProperty):
                    _cachedProperties.Add(baseProperty);
                    break;
                case EventDefinition baseEvent when !_cachedEvents.Contains(baseEvent):
                    _cachedEvents.Add(baseEvent);
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the base members that the given member overrides.
    /// </summary>
    /// <param name="member">The member to find base overrides for.</param>
    /// <returns>Collection of base members that are overridden.</returns>
    private static IEnumerable<IMemberDefinition> GetBaseMembers(IMemberDefinition member)
    {
        var declaringType = member.DeclaringType;
        if (declaringType?.BaseType?.Resolve() is not TypeDefinition baseType)
            yield break;

        switch (member)
        {
            case MethodDefinition method:
                foreach (var baseMethod in baseType.Methods)
                {
                    if (IsMethodOverride(method, baseMethod))
                        yield return baseMethod;
                }
                break;
            case PropertyDefinition property:
                foreach (var baseProperty in baseType.Properties)
                {
                    if (baseProperty.Name.Equals(property.Name))
                        yield return baseProperty;
                }

                break;
            case EventDefinition eventDef:
                foreach (var baseEvent in baseType.Events)
                {
                    if (baseEvent.Name.Equals(eventDef.Name))
                        yield return baseEvent;
                }
                break;
        }
    }

    /// <summary>
    /// Determines if a method overrides a base method.
    /// </summary>
    /// <param name="derivedMethod">The derived method.</param>
    /// <param name="baseMethod">The potential base method.</param>
    /// <returns>True if the derived method overrides the base method.</returns>
    private static bool IsMethodOverride(MethodDefinition derivedMethod, MethodDefinition baseMethod)
    {
        if (!derivedMethod.Name.Equals(baseMethod.Name))
            return false;

        if (derivedMethod.Signature?.ParameterTypes.Count != baseMethod.Signature?.ParameterTypes.Count)
            return false;

        if (!derivedMethod.Signature?.ReturnType.Equals(baseMethod.Signature?.ReturnType) == true)
            return false;
        for (int i = 0; i < derivedMethod.Signature.ParameterTypes.Count; i++)
        {
            if (!derivedMethod.Signature.ParameterTypes[i].Equals(baseMethod.Signature.ParameterTypes[i]))
                return false;
        }

        return true;
    }
}