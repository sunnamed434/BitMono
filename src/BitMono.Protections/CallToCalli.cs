namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class CallToCalli : Protection
{
    public CallToCalli(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync()
    {
        var importer = Context.ModuleImporter;
        var runtimeMethodHandle = importer.ImportType(typeof(RuntimeMethodHandle)).ToTypeSignature(isValueType: true);
        var getTypeFromHandleMethod = importer.ImportMethod(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new[]
        {
            typeof(RuntimeTypeHandle)
        }));
        var getModuleMethod = importer.ImportMethod(typeof(Type).GetProperty(nameof(Type.Module)).GetMethod);
        var resolveMethodMethod = importer.ImportMethod(typeof(Module).GetMethod(nameof(Module.ResolveMethod), new[]
        {
            typeof(int)
        }));
        var getMethodHandleMethod = importer.ImportMethod(typeof(MethodBase).GetProperty(nameof(MethodBase.MethodHandle)).GetMethod);
        var getFunctionPointerMethod = importer.ImportMethod(typeof(RuntimeMethodHandle).GetMethod(nameof(RuntimeMethodHandle.GetFunctionPointer)));

        var module = Context.Module;
        var moduleType = module.GetOrCreateModuleType();
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method is not { CilMethodBody: { } body, DeclaringType.IsModuleType: false })
            {
                continue;
            }

            for (var i = 0; i < body.Instructions.Count; i++)
            {
                var instruction = body.Instructions[i];
                if (instruction.OpCode != CilOpCodes.Call)
                {
                    continue;
                }
                if (instruction.Operand is not IMethodDescriptor methodDescriptor)
                {
                    continue;
                }
                var callingMethod = methodDescriptor.Resolve();
                if (callingMethod?.Signature == null)
                {
                    continue;
                }
                if (module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata) == false)
                {
                    continue;
                }

                var runtimeMethodHandleLocal = new CilLocalVariable(runtimeMethodHandle);
                body.LocalVariables.Add(runtimeMethodHandleLocal);
                instruction.ReplaceWith(CilOpCodes.Ldtoken, moduleType);
                body.Instructions.InsertRange(i + 1,
                [
                    new CilInstruction(CilOpCodes.Call, getTypeFromHandleMethod),
                    new CilInstruction(CilOpCodes.Callvirt, getModuleMethod),
                    new CilInstruction(CilOpCodes.Ldc_I4, callingMethodMetadata.MetadataToken.ToInt32()),
                    new CilInstruction(CilOpCodes.Call, resolveMethodMethod),
                    new CilInstruction(CilOpCodes.Callvirt, getMethodHandleMethod),
                    new CilInstruction(CilOpCodes.Stloc, runtimeMethodHandleLocal),
                    new CilInstruction(CilOpCodes.Ldloca, runtimeMethodHandleLocal),
                    new CilInstruction(CilOpCodes.Call, getFunctionPointerMethod),
                    new CilInstruction(CilOpCodes.Calli, callingMethod.Signature.MakeStandAloneSignature())
                ]);
            }
        }
        return Task.CompletedTask;
    }
}