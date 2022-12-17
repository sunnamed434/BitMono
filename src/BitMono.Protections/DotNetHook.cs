namespace BitMono.Protections;

[ProtectionName(nameof(DotNetHook))]
public class DotNetHook : IStageProtection
{
    private readonly IInjector m_Injector;
    private readonly IRenamer m_Renamer;
    private readonly ILogger m_Logger;
    private readonly Random m_Random;

    public DotNetHook(
        IInjector injector,
        IRenamer renamer,
        ILogger logger)
    {
        m_Injector = injector;
        m_Renamer = renamer;
        m_Logger = logger.ForContext<DotNetHook>();
        m_Random = new Random();
    }

    public PipelineStages Stage => PipelineStages.ModuleWritten;

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var runtimeHookingTypeDef = context.RuntimeModuleDefMD.ResolveTypeDefOrThrow<Hooking>();
        var injectedHookingDnlibDefs = InjectHelper.Inject(runtimeHookingTypeDef, context.ModuleDefMD.GlobalType, context.ModuleDefMD).UpdateRowIds(context.ModuleDefMD);
        var redirectStubMethodDef = injectedHookingDnlibDefs.Single(i => i.Name.String.Equals(nameof(Hooking.RedirectStub))).ResolveMethodDefOrThrow();

        foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
        {
            foreach (var methodDef in typeDef.Methods.ToArray())
            {
                if (methodDef.HasBody)
                {
                    for (var i = 0; i < methodDef.Body.Instructions.Count; i++)
                    {
                        if (methodDef.Body.Instructions[i].OpCode == OpCodes.Call
                            && methodDef.Body.Instructions[i].Operand is MethodDef callingMethodDef
                            && callingMethodDef.HasBody
                            && callingMethodDef.ParamDefs.Any(p => p.IsIn || p.IsOut) == false)
                        {
                            var dummyMethod = new MethodDefUser(m_Renamer.RenameUnsafely(),
                                callingMethodDef.MethodSig, callingMethodDef.ImplAttributes, callingMethodDef.Attributes);
                            dummyMethod.IsStatic = true;
                            dummyMethod.Access = MethodAttributes.Assembly;
                            dummyMethod.UpdateRowId(context.ModuleDefMD);
                            context.ModuleDefMD.GlobalType.Methods.Add(dummyMethod);
                            foreach (var paramDef in callingMethodDef.ParamDefs)
                            {
                                dummyMethod.ParamDefs.Add(new ParamDefUser(paramDef.Name, paramDef.Sequence, paramDef.Attributes));
                            }
                            dummyMethod.Body = new CilBody();
                            if (callingMethodDef.HasReturnType)
                            {
                                dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                            }
                            dummyMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                            var initializatorMethodDef = new MethodDefUser(m_Renamer.RenameUnsafely(),
                                MethodSig.CreateStatic(context.ModuleDefMD.CorLibTypes.Void), MethodAttributes.Assembly | MethodAttributes.Static);
                            initializatorMethodDef.Body = new CilBody();
                            initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Ldc_I4, dummyMethod.MDToken.ToInt32()));
                            initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Ldc_I4, callingMethodDef.MDToken.ToInt32()));
                            initializatorMethodDef.Body.Instructions.Add(new Instruction(OpCodes.Call, redirectStubMethodDef));
                            initializatorMethodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                            context.ModuleDefMD.GlobalType.Methods.Add(initializatorMethodDef);

                            methodDef.Body.Instructions[i].Operand = dummyMethod;
                            var globalTypeCctor = context.ModuleDefMD.GlobalType.FindOrCreateStaticConstructor();
                            var randomIndex = m_Random.Next(0, globalTypeCctor.Body.Instructions.CountWithoutRet());
                            globalTypeCctor.Body.Instructions.Insert(randomIndex, new Instruction(OpCodes.Call, initializatorMethodDef));
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}