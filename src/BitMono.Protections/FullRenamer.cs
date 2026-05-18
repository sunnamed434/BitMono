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
        //  1. Interface methods within the obfuscation scope and their
        //     implementations are renamed TOGETHER with the same random
        //     name (rather than being skipped wholesale because of
        //     IsVirtual, which would break the API surface).
        //  2. Compiler-generated async state-machine types
        //     ("<MethodName>d__N") are renamed as well so the original
        //     method name does not leak through stack traces.
        //  3. True "override" methods (IsVirtual && !IsNewSlot) are left
        //     alone because they must keep the same name as the base
        //     class method they override.
        //  4. Implicit implementations of interfaces that live OUTSIDE
        //     the current module (IsVirtual && IsNewSlot && IsFinal but
        //     not picked up by phase 1) are skipped so we do not break
        //     the contract and trigger TypeLoadException at assembly
        //     load time.
        // -----------------------------------------------------------------

        var members = Context.Parameters.Members;
        var membersList = members.ToList();
        var allTypes = membersList.OfType<TypeDefinition>().ToList();
        var allMethods = membersList.OfType<MethodDefinition>().ToList();
        var allFields = membersList.OfType<FieldDefinition>().ToList();

        // Track which methods have already received a new name through
        // phase 1 so the standard loop below can skip them.
        var alreadyRenamed = new HashSet<MethodDefinition>();

        // Phase 1: collect interface methods in scope, reserve a random
        //          name for each, and apply the same name to every
        //          matching implementation that is also in scope.
        var concreteTypes = allTypes
            .Where(t => !t.IsInterface && !t.IsModuleType && !t.IsCompilerGenerated())
            .ToList();

        foreach (var iface in allTypes.Where(t => t.IsInterface && !t.IsCompilerGenerated()))
        {
            foreach (var ifaceMethod in iface.Methods.ToList())
            {
                if (ifaceMethod.IsConstructor) continue;
                if (ifaceMethod.IsCompilerGenerated()) continue;
                if (!ShouldRenameMethodName(ifaceMethod)) continue;

                var groupName = _renamer.RenameUnsafely();

                // Capture the original name so we can match implementations
                // by name + signature BEFORE we overwrite the interface name.
                var originalIfaceName = ifaceMethod.Name?.Value;

                ifaceMethod.Name = groupName;
                alreadyRenamed.Add(ifaceMethod);
                RenameParametersOf(ifaceMethod);
                RenameAsyncStateMachineFor(ifaceMethod, originalIfaceName);

                // Find matching implementations.
                foreach (var concrete in concreteTypes)
                {
                    if (!ImplementsInterface(concrete, iface)) continue;

                    foreach (var implMethod in concrete.Methods.ToList())
                    {
                        if (implMethod.IsConstructor) continue;
                        if (implMethod.IsCompilerGenerated()) continue;
                        if (alreadyRenamed.Contains(implMethod)) continue;
                        if (originalIfaceName == null) continue;
                        if (implMethod.Name?.Value != originalIfaceName) continue;
                        if (!SignatureCompatible(implMethod, ifaceMethod)) continue;

                        var originalImplName = implMethod.Name?.Value;
                        implMethod.Name = groupName;
                        alreadyRenamed.Add(implMethod);
                        RenameParametersOf(implMethod);
                        RenameAsyncStateMachineFor(implMethod, originalImplName);
                    }
                }
            }
        }

        // Phase 2: standard rename loop for everything that was not
        //          handled by phase 1.
        foreach (var method in allMethods)
        {
            if (alreadyRenamed.Contains(method)) continue;
            if (method.DeclaringType?.IsModuleType == true) continue;
            if (method.IsConstructor) continue;
            if (method.IsCompilerGenerated()) continue;

            // Skip true overrides: IsVirtual && !IsNewSlot means the
            // method reuses an inherited vtable slot (override keyword
            // in C#). Renaming it would break the link to the base
            // class method.
            if (method.IsVirtual && !method.IsNewSlot) continue;

            // Skip implementations of external interfaces. The C#
            // compiler marks implicit interface impls with
            // IsVirtual+IsNewSlot+IsFinal+IsHideBySig. If phase 1 did
            // not pick them up (alreadyRenamed check above), the
            // corresponding interface lives in another assembly. If we
            // rename the impl, the contract breaks and the CLR throws
            // at assembly load time:
            //   System.TypeLoadException: Method '...' in type '...'
            //   does not have an implementation.
            if (method.IsVirtual && method.IsNewSlot && method.IsFinal) continue;

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

        // Type renames - unchanged from the original logic.
        foreach (var type in allTypes)
        {
            if (type.IsModuleType) continue;
            if (type.IsCompilerGenerated()) continue;
            _renamer.Rename(type);
        }

        // Field renames - unchanged from the original logic.
        foreach (var field in allFields)
        {
            if (field.DeclaringType?.IsModuleType == true) continue;
            if (field.IsCompilerGenerated()) continue;
            _renamer.Rename(field);
        }

        return Task.CompletedTask;
    }

    private static bool ShouldRenameMethodName(MethodDefinition method)
    {
        if (method.IsCompilerGenerated()) return false;
        if (method.IsConstructor) return false;
        return true;
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
        if (declaring == null) return;
        if (declaring.NestedTypes == null) return;

        // Compiler-generated state-machine types are named
        // "<MethodName>d__N" (async/await) or "<MethodName>b__N_M"
        // (lambdas). Once we know the original method name, we can find
        // the related nested types and rename them along with it.
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
        if (type.Interfaces == null) return false;
        foreach (var implemented in type.Interfaces)
        {
            var resolved = implemented.Interface?.Resolve();
            if (resolved == iface) return true;
        }
        // Also walk the base-type chain.
        var baseTypeDef = type.BaseType?.Resolve();
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
        var aRet = a.Signature.ReturnType?.FullName;
        var bRet = b.Signature.ReturnType?.FullName;
        if (!string.Equals(aRet, bRet, StringComparison.Ordinal)) return false;
        for (int i = 0; i < a.Signature.ParameterTypes.Count; i++)
        {
            var aParam = a.Signature.ParameterTypes[i]?.FullName;
            var bParam = b.Signature.ParameterTypes[i]?.FullName;
            if (!string.Equals(aParam, bParam, StringComparison.Ordinal)) return false;
        }
        return true;
    }
}
