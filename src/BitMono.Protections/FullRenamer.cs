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
        //  Behaviour:
        //
        //  1. A (non-generic) interface method in scope is renamed TOGETHER
        //     with every one of its in-scope implementations using a single
        //     shared name. The rename is atomic and gated: it only happens
        //     when at least one implementation was found and every member of
        //     the group passes the rename safety gate (Renamer.CanRename),
        //     so the interface <-> implementation contract stays consistent
        //     and configured/critical members are preserved.
        //  2. Compiler-generated async/iterator state-machine types
        //     ("<Method>d__N") are renamed alongside their method so the
        //     original name does not leak through stack traces.
        //  3. All other virtual methods are skipped (as in the original
        //     FullRenamer) so override chains and external-interface
        //     contracts are never broken.
        // -----------------------------------------------------------------

        // Generic-instance member refs (e.g. Template<int>.Do()) carry their own name string that
        // renaming the definition won't update; capture them now and re-sync after. See #220.
        var memberReferences = CollectModuleMemberReferences(Context.Module);

        var membersList = Context.Parameters.Members.ToList();
        var allTypes = membersList.OfType<TypeDefinition>().ToList();
        var allMethods = membersList.OfType<MethodDefinition>().ToList();
        var allFields = membersList.OfType<FieldDefinition>().ToList();

        var alreadyRenamed = new HashSet<MethodDefinition>();

        var concreteTypes = allTypes
            .Where(t => !t.IsInterface && !t.IsModuleType && !t.IsCompilerGenerated())
            .ToList();

        // Phase 1: interface methods together with their implementations.
        foreach (var iface in allTypes.Where(t => t.IsInterface && !t.IsCompilerGenerated()))
        {
            // Generic interfaces are skipped: matching implementations by signature is
            // unreliable across generic instantiations (e.g. T vs the concrete argument),
            // so we cannot be confident we found every implementation.
            if (iface.GenericParameters.Count > 0)
            {
                continue;
            }
            foreach (var ifaceMethod in iface.Methods.ToList())
            {
                if (ifaceMethod.IsConstructor) continue;
                if (ifaceMethod.IsCompilerGenerated()) continue;
                if (alreadyRenamed.Contains(ifaceMethod)) continue;

                var originalName = ifaceMethod.Name?.Value;
                if (string.IsNullOrEmpty(originalName)) continue;

                // Collect the interface method and every matching implementation in scope.
                var group = new List<MethodDefinition> { ifaceMethod };
                foreach (var concrete in concreteTypes)
                {
                    if (!ImplementsInterface(concrete, iface)) continue;
                    foreach (var implMethod in concrete.Methods.ToList())
                    {
                        if (implMethod.IsConstructor) continue;
                        if (implMethod.IsCompilerGenerated()) continue;
                        if (alreadyRenamed.Contains(implMethod)) continue;
                        if (implMethod.Name?.Value != originalName) continue;
                        if (!SignatureCompatible(implMethod, ifaceMethod)) continue;
                        group.Add(implMethod);
                    }
                }

                // Require at least one in-scope implementation: an interface method with
                // none found is likely implemented outside the obfuscation scope, so
                // renaming it would break that external contract.
                if (group.Count == 1) continue;

                // Atomic + gated: only rename when every member can be renamed safely.
                if (group.Any(method => !_renamer.CanRename(method))) continue;

                var groupName = _renamer.RenameUnsafely();
                foreach (var method in group)
                {
                    method.Name = groupName;
                    alreadyRenamed.Add(method);
                    RenameParametersOf(method);
                    RenameAsyncStateMachineFor(method, originalName);
                }
            }
        }

        // Phase 2: everything else, skipping all virtual methods so override chains and
        // external-interface implementations are never broken (interface methods and their
        // implementations were already handled atomically in phase 1).
        foreach (var method in allMethods)
        {
            if (alreadyRenamed.Contains(method)) continue;
            if (method.DeclaringType?.IsModuleType == true) continue;
            if (method.IsConstructor) continue;
            if (method.IsCompilerGenerated()) continue;
            if (method.IsVirtual) continue;

            var originalName = method.Name?.Value;
            _renamer.Rename(method);
            RenameAsyncStateMachineFor(method, originalName);

            if (!method.HasParameters())
            {
                continue;
            }
            foreach (var parameter in method.Parameters)
            {
                if (parameter.Definition == null)
                {
                    continue;
                }
                _renamer.Rename(parameter.Definition);
            }
        }

        foreach (var type in allTypes)
        {
            if (type.IsModuleType) continue;
            if (type.IsCompilerGenerated()) continue;
            _renamer.Rename(type);
        }

        foreach (var field in allFields)
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

    private static bool ImplementsInterface(TypeDefinition type, TypeDefinition iface)
    {
        if (type.Interfaces != null)
        {
            foreach (var implemented in type.Interfaces)
            {
                if (implemented.Interface?.ResolveOrNull() == iface) return true;
            }
        }
        // Also walk the base-type chain.
        var baseTypeDef = type.BaseType?.ResolveOrNull();
        if (baseTypeDef != null && baseTypeDef != type)
        {
            return ImplementsInterface(baseTypeDef, iface);
        }
        return false;
    }

    private static bool SignatureCompatible(MethodDefinition a, MethodDefinition b)
    {
        if (a.Signature == null || b.Signature == null) return false;
        if (a.Signature.ParameterTypes.Count != b.Signature.ParameterTypes.Count) return false;
        if (a.Signature.GenericParameterCount != b.Signature.GenericParameterCount) return false;
        if (!string.Equals(a.Signature.ReturnType?.FullName, b.Signature.ReturnType?.FullName, StringComparison.Ordinal)) return false;
        for (int i = 0; i < a.Signature.ParameterTypes.Count; i++)
        {
            if (!string.Equals(a.Signature.ParameterTypes[i]?.FullName, b.Signature.ParameterTypes[i]?.FullName, StringComparison.Ordinal)) return false;
        }
        return true;
    }
}
