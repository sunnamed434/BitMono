namespace BitMono.Core.Renaming;

/// <summary>
/// Groups every method in a module by the vtable slot it occupies: an interface method, its
/// implementations (implicit and explicit) and any override chain all land in one slot and must
/// share a name. A slot that reaches a method, type or interface outside the module - or one that
/// cannot be resolved - is flagged <see cref="MethodSlot.TiedToExternal"/> so the renamer leaves it
/// alone. This lets the renamer safely rename virtual/interface methods whose whole slot is in
/// scope instead of skipping every virtual. Generic interfaces (IComparable&lt;T&gt; etc.) are
/// handled, not skipped, via AsmResolver's <see cref="SignatureComparer"/> + generic instantiation.
/// </summary>
public sealed class MethodSlotGrouper
{
    private static readonly SignatureComparer Signatures = new();
    private readonly Dictionary<MethodDefinition, MethodDefinition> _parent = new();
    private readonly HashSet<MethodDefinition> _external = new();

    public IReadOnlyList<MethodSlot> Group(ModuleDefinition module)
    {
        _parent.Clear();
        _external.Clear();

        var types = module.GetAllTypes().ToList();
        foreach (var type in types)
        {
            foreach (var method in type.Methods)
            {
                Add(method);
            }
        }

        foreach (var type in types)
        {
            AnalyzeExplicitOverrides(type, module);
            AnalyzeInterfaceImplementations(type, module);
            AnalyzeOverrideChains(type, module);
        }

        var slots = new List<MethodSlot>();
        foreach (var group in _parent.Keys.GroupBy(Find))
        {
            var members = group.ToList();
            slots.Add(new MethodSlot(members, members.Any(_external.Contains)));
        }
        return slots;
    }

    // (a) Explicit interface implementations carry a .override row linking the implementing body to
    // the interface declaration directly - no signature matching needed (works for generics too).
    private void AnalyzeExplicitOverrides(TypeDefinition type, ModuleDefinition module)
    {
        foreach (var implementation in type.MethodImplementations)
        {
            if (implementation.Body?.ResolveOrNull() is not { } body || !InModule(body, module))
            {
                continue;
            }
            if (implementation.Declaration?.ResolveOrNull() is not { } declaration)
            {
                _external.Add(body); // interface lives in an assembly we can't resolve
                continue;
            }
            if (InModule(declaration, module))
            {
                Union(body, declaration);
            }
            else
            {
                _external.Add(body);
            }
        }
    }

    // (b) Implicit interface implementations: match each interface method (with its generic args
    // substituted) to a method on the type or its base chain.
    private void AnalyzeInterfaceImplementations(TypeDefinition type, ModuleDefinition module)
    {
        foreach (var (interfaceType, context) in EnumerateInterfaces(type))
        {
            if (interfaceType == null)
            {
                // Interface in an unresolved assembly: keep every candidate implementation.
                foreach (var method in type.Methods)
                {
                    if (method.IsVirtual)
                    {
                        _external.Add(method);
                    }
                }
                continue;
            }

            var interfaceInModule = ReferenceEquals(interfaceType.DeclaringModule, module);
            foreach (var interfaceMethod in interfaceType.Methods)
            {
                if (interfaceMethod.IsStatic || !interfaceMethod.IsVirtual || interfaceMethod.Signature == null)
                {
                    continue;
                }
                var wanted = interfaceMethod.Signature.InstantiateGenericTypes(context);
                if (FindImplementation(type, interfaceMethod.Name, wanted) is not { } implementation)
                {
                    continue;
                }
                if (interfaceInModule)
                {
                    Union(implementation, interfaceMethod);
                }
                else
                {
                    _external.Add(implementation);
                }
            }
        }
    }

    // (c) Virtual overrides reuse their base type's slot; walk the base chain (substituting generic
    // args) to find the method being overridden.
    private void AnalyzeOverrideChains(TypeDefinition type, ModuleDefinition module)
    {
        foreach (var method in type.Methods)
        {
            if (!method.IsVirtual || !method.IsReuseSlot || method.Signature == null)
            {
                continue;
            }
            var (baseMethod, hitUnresolved) = FindInBaseChain(type.BaseType, method.Name, method.Signature);
            if (baseMethod != null)
            {
                if (InModule(baseMethod, module))
                {
                    Union(method, baseMethod);
                }
                else
                {
                    _external.Add(method); // overrides an external virtual (e.g. object.ToString)
                }
            }
            else if (hitUnresolved)
            {
                _external.Add(method);
            }
        }
    }

    private static MethodDefinition? FindImplementation(TypeDefinition type, Utf8String? name, MethodSignature wanted)
    {
        foreach (var current in type.GetTypeAndBaseTypes())
        {
            foreach (var method in current.Methods)
            {
                if (method.IsVirtual && method.Name == name && method.Signature != null
                    && Signatures.Equals(method.Signature, wanted))
                {
                    return method;
                }
            }
        }
        return null;
    }

    private static (MethodDefinition? Method, bool HitUnresolved) FindInBaseChain(
        ITypeDefOrRef? baseType, Utf8String? name, MethodSignature derivedSignature)
    {
        var context = default(GenericContext);
        while (baseType != null)
        {
            if (baseType is TypeSpecification { Signature: GenericInstanceTypeSignature genericBase })
            {
                context = GenericContext.FromType((GenericInstanceTypeSignature)genericBase.InstantiateGenericTypes(context));
            }
            if (baseType.ResolveOrNull() is not { } definition)
            {
                return (null, true); // base type in an assembly we can't resolve
            }
            foreach (var method in definition.Methods)
            {
                if (!method.IsVirtual || method.Name != name || method.Signature == null)
                {
                    continue;
                }
                if (Signatures.Equals(method.Signature.InstantiateGenericTypes(context), derivedSignature))
                {
                    return (method, false);
                }
            }
            baseType = definition.BaseType;
        }
        return (null, false);
    }

    // Yields every interface the type implements - directly, through its base chain, and through
    // interface inheritance - with the generic context needed to substitute its type args.
    private static IEnumerable<(TypeDefinition? Interface, GenericContext Context)> EnumerateInterfaces(TypeDefinition type)
    {
        var visited = new HashSet<TypeDefinition>();
        for (var current = type; current != null; current = current.BaseType?.ResolveOrNull())
        {
            foreach (var implementation in current.Interfaces)
            {
                foreach (var entry in Walk(implementation.Interface, default, visited))
                {
                    yield return entry;
                }
            }
        }
    }

    private static IEnumerable<(TypeDefinition?, GenericContext)> Walk(
        ITypeDefOrRef? interfaceType, GenericContext outer, HashSet<TypeDefinition> visited)
    {
        if (interfaceType == null)
        {
            yield break;
        }
        var context = interfaceType is TypeSpecification { Signature: GenericInstanceTypeSignature generic }
            ? GenericContext.FromType((GenericInstanceTypeSignature)generic.InstantiateGenericTypes(outer))
            : default;
        var definition = interfaceType.ResolveOrNull();
        yield return (definition, context);
        if (definition == null || !visited.Add(definition))
        {
            yield break;
        }
        foreach (var inherited in definition.Interfaces)
        {
            foreach (var entry in Walk(inherited.Interface, context, visited))
            {
                yield return entry;
            }
        }
    }

    private static bool InModule(MethodDefinition method, ModuleDefinition module)
    {
        return ReferenceEquals(method.DeclaringModule, module);
    }

    private void Add(MethodDefinition method)
    {
        if (!_parent.ContainsKey(method))
        {
            _parent[method] = method;
        }
    }

    private MethodDefinition Find(MethodDefinition method)
    {
        var root = method;
        while (!ReferenceEquals(_parent[root], root))
        {
            root = _parent[root];
        }
        while (!ReferenceEquals(_parent[method], root))
        {
            var next = _parent[method];
            _parent[method] = root;
            method = next;
        }
        return root;
    }

    private void Union(MethodDefinition left, MethodDefinition right)
    {
        Add(left);
        Add(right);
        var leftRoot = Find(left);
        var rightRoot = Find(right);
        if (!ReferenceEquals(leftRoot, rightRoot))
        {
            _parent[leftRoot] = rightRoot;
        }
    }
}

public sealed class MethodSlot
{
    public MethodSlot(IReadOnlyList<MethodDefinition> methods, bool tiedToExternal)
    {
        Methods = methods;
        TiedToExternal = tiedToExternal;
    }

    public IReadOnlyList<MethodDefinition> Methods { get; }

    /// <summary>
    /// True when the slot reaches a method, type or interface outside the module (or one that could
    /// not be resolved). Renaming any member would break that external contract, so the whole slot
    /// must keep its original name.
    /// </summary>
    public bool TiedToExternal { get; }
}
