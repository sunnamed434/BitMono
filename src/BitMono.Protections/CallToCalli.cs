namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class CallToCalli : Protection
{
    public CallToCalli(ProtectionContext context) : base(context)
    {
    }

    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        var runtimeMethodHandle = Context.Importer.ImportType(typeof(RuntimeMethodHandle)).ToTypeSignature(isValueType: true);
        var getTypeFromHandleMethod = Context.Importer.ImportMethod(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[]
        {
            typeof(RuntimeTypeHandle)
        }));
        var getModuleMethod = Context.Importer.ImportMethod(typeof(Type).GetProperty(nameof(Type.Module)).GetMethod);
        var resolveMethodMethod = Context.Importer.ImportMethod(typeof(Module).GetMethod(nameof(Module.ResolveMethod), new Type[]
        {
            typeof(int)
        }));
        var getMethodHandleMethod = Context.Importer.ImportMethod(typeof(MethodBase).GetProperty(nameof(MethodBase.MethodHandle)).GetMethod);
        var getFunctionPointerMethod = Context.Importer.ImportMethod(typeof(RuntimeMethodHandle).GetMethod(nameof(RuntimeMethodHandle.GetFunctionPointer)));

        var moduleType = Context.Module.GetOrCreateModuleType();
        foreach (var method in parameters.Members.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body && method.DeclaringType?.IsModuleType == false)
            {
                for (var i = 0; i < body.Instructions.Count; i++)
                {
                    var instruction = body.Instructions[i];
                    if (instruction.OpCode == CilOpCodes.Call && instruction.Operand is IMethodDescriptor methodDescriptor)
                    {
                        var callingMethod = methodDescriptor.Resolve();
                        if (callingMethod != null)
                        {
                            if (Context.Module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata))
                            {
                                var runtimeMethodHandleLocal = new CilLocalVariable(runtimeMethodHandle);
                                body.LocalVariables.Add(runtimeMethodHandleLocal);
                                instruction.ReplaceWith(CilOpCodes.Ldtoken, moduleType);
                                body.Instructions.InsertRange(i + 1, new CilInstruction[]
                                {
                                    new CilInstruction(CilOpCodes.Call, getTypeFromHandleMethod),
                                    new CilInstruction(CilOpCodes.Callvirt, getModuleMethod),
                                    new CilInstruction(CilOpCodes.Ldc_I4, callingMethodMetadata.MetadataToken.ToInt32()),
                                    new CilInstruction(CilOpCodes.Call, resolveMethodMethod),
                                    new CilInstruction(CilOpCodes.Callvirt, getMethodHandleMethod),
                                    new CilInstruction(CilOpCodes.Stloc, runtimeMethodHandleLocal),
                                    new CilInstruction(CilOpCodes.Ldloca, runtimeMethodHandleLocal),
                                    new CilInstruction(CilOpCodes.Call, getFunctionPointerMethod),
                                    new CilInstruction(CilOpCodes.Calli, callingMethod.Signature.MakeStandAloneSignature())
                                });
                            }
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}