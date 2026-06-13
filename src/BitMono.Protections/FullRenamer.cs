namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Model | MemberInclusionFlags.Reflection)]
public class FullRenamer : Protection
{
    private readonly Renamer _renamer;

    public FullRenamer(Renamer renamer, IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
        _renamer = renamer;
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

        return Task.CompletedTask;
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
