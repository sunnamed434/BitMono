namespace BitMono.Protections;

[UsedImplicitly]
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
        var runtimeMethodHandle = Context.ModuleImporter.ImportType(typeof(RuntimeMethodHandle)).ToTypeSignature(isValueType: true);
        var getTypeFromHandleMethod = Context.ModuleImporter.ImportMethod(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new[]
        {
            typeof(RuntimeTypeHandle)
        }));
        var getModuleMethod = Context.ModuleImporter.ImportMethod(typeof(Type).GetProperty(nameof(Type.Module)).GetMethod);
        var resolveMethodMethod = Context.ModuleImporter.ImportMethod(typeof(Module).GetMethod(nameof(Module.ResolveMethod), new[]
        {
            typeof(int)
        }));
        var getMethodHandleMethod = Context.ModuleImporter.ImportMethod(typeof(MethodBase).GetProperty(nameof(MethodBase.MethodHandle)).GetMethod);
        var getFunctionPointerMethod = Context.ModuleImporter.ImportMethod(typeof(RuntimeMethodHandle).GetMethod(nameof(RuntimeMethodHandle.GetFunctionPointer)));

        var moduleType = Context.Module.GetOrCreateModuleType();
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body && method.DeclaringType?.IsModuleType == false)
            {
                for (var i = 0; i < body.Instructions.Count; i++)
                {
                    var instruction = body.Instructions[i];
                    if (instruction.OpCode == CilOpCodes.Call && instruction.Operand is IMethodDescriptor methodDescriptor)
                    {
                        var callingMethod = methodDescriptor.Resolve();
                        if (callingMethod?.Signature != null)
                        {
                            if (Context.Module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata))
                            {
                                var runtimeMethodHandleLocal = new CilLocalVariable(runtimeMethodHandle);
                                body.LocalVariables.Add(runtimeMethodHandleLocal);
                                instruction.ReplaceWith(CilOpCodes.Ldtoken, moduleType);
                                body.Instructions.InsertRange(i + 1, new CilInstruction[]
                                {
                                    new(CilOpCodes.Call, getTypeFromHandleMethod),
                                    new(CilOpCodes.Callvirt, getModuleMethod),
                                    new(CilOpCodes.Ldc_I4, callingMethodMetadata.MetadataToken.ToInt32()),
                                    new(CilOpCodes.Call, resolveMethodMethod),
                                    new(CilOpCodes.Callvirt, getMethodHandleMethod),
                                    new(CilOpCodes.Stloc, runtimeMethodHandleLocal),
                                    new(CilOpCodes.Ldloca, runtimeMethodHandleLocal),
                                    new(CilOpCodes.Call, getFunctionPointerMethod),
                                    new(CilOpCodes.Calli, callingMethod.Signature.MakeStandAloneSignature())
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