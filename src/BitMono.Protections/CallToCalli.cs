using System.Reflection;

namespace BitMono.Protections;

public class CallToCalli : IStageProtection
{
    private readonly IInjector m_Injector;
    private readonly IRenamer m_Renamer;
    private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
    private readonly ILogger m_Logger;

    public CallToCalli(
        IInjector injector,
        IRenamer renamer,
        DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
        ILogger logger)
    {
        m_Injector = injector;
        m_Renamer = renamer;
        m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
        m_Logger = logger.ForContext<CallToCalli>();
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

        var globalType = context.Module.GetOrCreateModuleType(); 
        foreach (var method in parameters.Targets.OfType<MethodDefinition>()) // Gets all of the sorted MethodDefinitions from the custom context (Targets are implements the IMemberDefinition)
        {
            if (method.HasMethodBody)
            {
                for (var i = 0; i < method.CilMethodBody.Instructions.Count; i++)
                {
                    if (method.CilMethodBody.Instructions[i].OpCode == CilOpCodes.Call
                        && method.CilMethodBody.Instructions[i].Operand is IMethodDescriptor methodDescriptor)
                    {
                        var methodDefinition = methodDescriptor.Resolve();
                        if (methodDefinition != null)
                        {
                            var runtimeMethodHandleLocal = new CilLocalVariable(runtimeMethodHandle);
                            method.CilMethodBody.LocalVariables.Add(runtimeMethodHandleLocal);
                            method.CilMethodBody.Instructions[i].ReplaceWith(CilOpCodes.Ldtoken, globalType);
                            method.CilMethodBody.Instructions.Insert(i + 1, new CilInstruction(CilOpCodes.Call, getTypeFromHandleMethod));
                            method.CilMethodBody.Instructions.Insert(i + 2, new CilInstruction(CilOpCodes.Callvirt, getModuleMethod));
                            method.CilMethodBody.Instructions.Insert(i + 3, new CilInstruction(CilOpCodes.Ldc_I4, methodDefinition.MetadataToken.ToInt32()));
                            method.CilMethodBody.Instructions.Insert(i + 4, new CilInstruction(CilOpCodes.Call, resolveMethodMethod));
                            method.CilMethodBody.Instructions.Insert(i + 5, new CilInstruction(CilOpCodes.Callvirt, getMethodHandleMethod));
                            method.CilMethodBody.Instructions.Insert(i + 6, new CilInstruction(CilOpCodes.Stloc, runtimeMethodHandleLocal));
                            method.CilMethodBody.Instructions.Insert(i + 7, new CilInstruction(CilOpCodes.Ldloca, runtimeMethodHandleLocal));
                            method.CilMethodBody.Instructions.Insert(i + 8, new CilInstruction(CilOpCodes.Call, getFunctionPointerMethod));
                            method.CilMethodBody.Instructions.Insert(i + 9, new CilInstruction(CilOpCodes.Calli, methodDefinition.Signature.MakeStandAloneSignature()));
                            i += 9;
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}