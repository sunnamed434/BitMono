namespace BitMono.Protections;

public class CallToCalli : IStageProtection
{
    private readonly IInjector m_Injector;
    private readonly IRenamer m_Renamer;
    private readonly CriticalAnalyzer m_DnlibDefCriticalAnalyzer;

    public CallToCalli(IInjector injector, IRenamer renamer, CriticalAnalyzer dnlibDefCriticalAnalyzer)
    {
        m_Injector = injector;
        m_Renamer = renamer;
        m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
    }

    public PipelineStages Stage => PipelineStages.ModuleWritten;

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var runtimeMethodHandle = context.Importer.ImportType(typeof(RuntimeMethodHandle)).ToTypeSignature(isValueType: true);
        var getTypeFromHandleMethod = context.Importer.ImportMethod(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[]
        {
            typeof(RuntimeTypeHandle)
        }));
        var getModuleMethod = context.Importer.ImportMethod(typeof(Type).GetProperty(nameof(Type.Module)).GetMethod);
        var resolveMethodMethod = context.Importer.ImportMethod(typeof(Module).GetMethod(nameof(Module.ResolveMethod), new Type[]
        {
            typeof(int)
        }));
        var getMethodHandleMethod = context.Importer.ImportMethod(typeof(MethodBase).GetProperty(nameof(MethodBase.MethodHandle)).GetMethod);
        var getFunctionPointerMethod = context.Importer.ImportMethod(typeof(RuntimeMethodHandle).GetMethod(nameof(RuntimeMethodHandle.GetFunctionPointer)));

        var moduleType = context.Module.GetOrCreateModuleType(); 
        foreach (var method in parameters.Targets.OfType<MethodDefinition>())
        {
            if (method.HasMethodBody && method.DeclaringType != moduleType
                && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(method))
            {
                for (var i = 0; i < method.CilMethodBody.Instructions.Count; i++)
                {
                    if (method.CilMethodBody.Instructions[i].OpCode == CilOpCodes.Call
                        && method.CilMethodBody.Instructions[i].Operand is IMethodDescriptor methodDescriptor)
                    {
                        var callingMethod = methodDescriptor.Resolve();
                        if (callingMethod != null)
                        {
                            if (context.Module.TryLookupMember(callingMethod.MetadataToken, out var callingMethodMetadata))
                            {
                                var runtimeMethodHandleLocal = new CilLocalVariable(runtimeMethodHandle);
                                method.CilMethodBody.LocalVariables.Add(runtimeMethodHandleLocal);
                                method.CilMethodBody.Instructions[i].ReplaceWith(CilOpCodes.Ldtoken, moduleType);
                                method.CilMethodBody.Instructions.Insert(i + 1, new CilInstruction(CilOpCodes.Call, getTypeFromHandleMethod));
                                method.CilMethodBody.Instructions.Insert(i + 2, new CilInstruction(CilOpCodes.Callvirt, getModuleMethod));
                                method.CilMethodBody.Instructions.Insert(i + 3, new CilInstruction(CilOpCodes.Ldc_I4, callingMethodMetadata.MetadataToken.ToInt32()));
                                method.CilMethodBody.Instructions.Insert(i + 4, new CilInstruction(CilOpCodes.Call, resolveMethodMethod));
                                method.CilMethodBody.Instructions.Insert(i + 5, new CilInstruction(CilOpCodes.Callvirt, getMethodHandleMethod));
                                method.CilMethodBody.Instructions.Insert(i + 6, new CilInstruction(CilOpCodes.Stloc, runtimeMethodHandleLocal));
                                method.CilMethodBody.Instructions.Insert(i + 7, new CilInstruction(CilOpCodes.Ldloca, runtimeMethodHandleLocal));
                                method.CilMethodBody.Instructions.Insert(i + 8, new CilInstruction(CilOpCodes.Call, getFunctionPointerMethod));
                                method.CilMethodBody.Instructions.Insert(i + 9, new CilInstruction(CilOpCodes.Calli, callingMethod.Signature.MakeStandAloneSignature()));
                                i += 9;
                            }
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}