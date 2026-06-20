using BitMono.Core.Analyzing.Baml;

namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Model | MemberInclusionFlags.Reflection | MemberInclusionFlags.Baml)]
public class FullRenamer : Protection
{
    private readonly Renamer _renamer;
    private readonly WpfBamlContextAccessor _bamlContextAccessor;

    public FullRenamer(Renamer renamer, WpfBamlContextAccessor bamlContextAccessor, IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
        _renamer = renamer;
        _bamlContextAccessor = bamlContextAccessor;
    }

    public override Task ExecuteAsync()
    {
        // -----------------------------------------------------------------
        //  Methods are renamed by vtable slot (see MethodSlotGrouper): an
        //  interface method, all of its implementations (implicit and
        //  explicit) and every override in a chain share ONE name, so the
        //  contract stays consistent. A slot is renamed only when it is
        //  entirely in our control - it touches nothing external/unresolved
        //  and every member is in scope and not critical - otherwise the
        //  whole slot keeps its original name. This replaces the old "skip
        //  every virtual" pass, so fully in-scope virtual/interface methods
        //  now get renamed instead of leaking their original names.
        // -----------------------------------------------------------------

        // Generic-instance member refs (e.g. Template<int>.Do()) carry their own name string that
        // renaming the definition won't update; capture them now and re-sync after. See #220.
        var memberReferences = CollectModuleMemberReferences(Context.Module);

        var members = Context.Parameters.Members;
        var inScope = new HashSet<IMetadataMember>(members);

        foreach (var slot in new MethodSlotGrouper().Group(Context.Module))
        {
            if (slot.TiedToExternal)
            {
                continue;
            }
            if (slot.Methods.Any(method => !CanRenameInSlot(method, inScope)))
            {
                continue;
            }
            var name = _renamer.RenameUnsafely();
            foreach (var method in slot.Methods)
            {
                var originalName = method.Name?.Value;
                method.Name = name;
                RenameParametersOf(method);
                RenameAsyncStateMachineFor(method, originalName);
            }
        }

        foreach (var type in members.OfType<TypeDefinition>())
        {
            if (type.IsModuleType) continue;
            if (type.IsCompilerGenerated()) continue;
            _renamer.Rename(type);
        }

        foreach (var field in members.OfType<FieldDefinition>())
        {
            if (field.DeclaringType?.IsModuleType == true) continue;
            if (field.IsCompilerGenerated()) continue;
            _renamer.Rename(field);
        }

        SyncMemberReferenceNames(memberReferences);

        // Rename the type names XAML references and update the BAML to match (their members are kept
        // by the Baml exclusion). No-op unless WpfBamlRewrite is on. See issue #212.
        _bamlContextAccessor.GetContext()?.ApplyRewrite(Context.Module, _renamer);
        return Task.CompletedTask;
    }

    // A slot member may be renamed only when it is in obfuscation scope and passes the same gates the
    // single-member rename path applies: not a constructor, not compiler-generated, not on the module
    // type, and not name-critical.
    private bool CanRenameInSlot(MethodDefinition method, HashSet<IMetadataMember> inScope)
    {
        return inScope.Contains(method)
            && !method.IsConstructor
            && !method.IsCompilerGenerated()
            && method.DeclaringType?.IsModuleType != true
            && _renamer.CanRename(method);
    }

    // Maps each MemberReference in the module's method bodies to the in-module definition it points
    // to, resolved before renaming while names still match.
    [SuppressMessage("Usage", "BITM0001:Iterate Context.Parameters.Members, not the module",
        Justification = "Builds a MemberReference->definition map to re-sync generic-instance names after renaming (#220); reads the module, doesn't obfuscate, so [DoNotResolve] filtering doesn't apply.")]
    private static Dictionary<MemberReference, IMemberDefinition> CollectModuleMemberReferences(ModuleDefinition module)
    {
        var moduleTypes = new HashSet<TypeDefinition>(module.GetAllTypes());
        var references = new Dictionary<MemberReference, IMemberDefinition>();
        foreach (var type in moduleTypes)
        {
            foreach (var method in type.Methods)
            {
                if (method.CilMethodBody is not { } body)
                {
                    continue;
                }
                foreach (var instruction in body.Instructions)
                {
                    var reference = instruction.Operand switch
                    {
                        MemberReference memberReference => memberReference,
                        MethodSpecification { Method: MemberReference memberReference } => memberReference,
                        _ => null
                    };
                    if (reference == null || references.ContainsKey(reference))
                    {
                        continue;
                    }
                    if (ResolveModuleMember(moduleTypes, reference) is { } definition)
                    {
                        references[reference] = definition;
                    }
                }
            }
        }
        return references;
    }

    private static IMemberDefinition? ResolveModuleMember(HashSet<TypeDefinition> moduleTypes, MemberReference reference)
    {
        // MemberReference is both an IMethodDescriptor and an IFieldDescriptor, so cast to the
        // matching one to pick the right ResolveOrNull overload.
        if (reference.IsMethod)
        {
            var method = ((IMethodDescriptor)reference).ResolveOrNull();
            return method?.DeclaringType != null && moduleTypes.Contains(method.DeclaringType) ? method : null;
        }
        if (reference.IsField)
        {
            var field = ((IFieldDescriptor)reference).ResolveOrNull();
            return field?.DeclaringType != null && moduleTypes.Contains(field.DeclaringType) ? field : null;
        }
        return null;
    }

    // Re-points each recorded reference at its definition's (possibly renamed) name.
    private static void SyncMemberReferenceNames(Dictionary<MemberReference, IMemberDefinition> references)
    {
        foreach (var pair in references)
        {
            pair.Key.Name = pair.Value.Name;
        }
    }

    private void RenameParametersOf(MethodDefinition method)
    {
        if (!method.HasParameters()) return;
        foreach (var parameter in method.Parameters)
        {
            if (parameter.Definition == null) continue;
            _renamer.Rename(parameter.Definition);
        }
    }

    private void RenameAsyncStateMachineFor(MethodDefinition method, string? originalName)
    {
        if (string.IsNullOrEmpty(originalName)) return;
        var declaring = method.DeclaringType;
        if (declaring?.NestedTypes == null) return;

        // Compiler-generated async/iterator state-machine types are named
        // "<MethodName>d__N". Once we know the original method name we can find the
        // related nested types and rename them so the name does not leak through them.
        // The state-machine TYPE NAME is compiler-generated and not part of any
        // contract (the IAsyncStateMachine implementation is by its methods, not its
        // name), so it is always safe to rename even though the standard type gate
        // would treat IAsyncStateMachine as a critical interface.
        var prefix = "<" + originalName + ">";
        foreach (var nested in declaring.NestedTypes.ToList())
        {
            var nestedName = nested.Name?.Value;
            if (nestedName == null) continue;
            if (!nestedName.StartsWith(prefix, StringComparison.Ordinal)) continue;
            nested.Name = _renamer.RenameUnsafely();
        }
    }
}
