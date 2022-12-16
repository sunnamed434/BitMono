namespace BitMono.Protections;

[Obsolete]
[ProtectionName(nameof(FieldsHiding))]
public class FieldsHiding : IStageProtection
{
    private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
    private readonly ILogger m_Logger;

    public FieldsHiding(
        DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
        ILogger logger)
    {
        m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
        m_Logger = logger.ForContext<FieldsHiding>();
    }

    public PipelineStages Stage => PipelineStages.ModuleWritten;

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var moduleTypeDef = context.Importer.Import(typeof(Module));

        var initializeArrayMethod = context.Importer.Import(typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.InitializeArray), new Type[]
        {
            typeof(Array),
            typeof(RuntimeFieldHandle)
        }));
        var getTypeFromHandleMethod = context.Importer.Import(typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[]
        {
            typeof(RuntimeTypeHandle)
        }));
        var getModuleMethod = context.Importer.Import(typeof(Type).GetProperty(nameof(Type.Module)).GetMethod);
        var resolveFieldMethod = context.Importer.Import(typeof(Module).GetMethod(nameof(Module.ResolveField), new Type[]
        {
            typeof(int)
        }));
        var getFieldHandleMethod = context.Importer.Import(typeof(FieldInfo).GetProperty(nameof(FieldInfo.FieldHandle)).GetMethod);

        foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
        {
            if (typeDef.HasFields)
            {
                foreach (var fieldDef in typeDef.Fields.ToArray())
                {
                    if (m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(fieldDef)
                        && fieldDef.HasFieldRVA)
                    {
                        var cctor = fieldDef.DeclaringType.FindOrCreateStaticConstructor();
                        for (int i = 0; i < cctor.Body.Instructions.Count; i++)
                        {
                            if (cctor.Body.Instructions[i].OpCode == OpCodes.Call)
                            {
                                cctor.Body.Instructions[i].OpCode = OpCodes.Nop;

                                cctor.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Ldtoken, moduleTypeDef));
                                cctor.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Call, getTypeFromHandleMethod));
                                cctor.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Callvirt, getModuleMethod));

                                cctor.Body.Instructions.Insert(i + 4, new Instruction(OpCodes.Ldc_I4, fieldDef.MDToken.ToInt32()));
                                cctor.Body.Instructions.Insert(i + 5, new Instruction(OpCodes.Callvirt, resolveFieldMethod));
                                cctor.Body.Instructions.Insert(i + 6, new Instruction(OpCodes.Callvirt, getFieldHandleMethod));

                                cctor.Body.Instructions.Insert(i + 7, new Instruction(OpCodes.Call, initializeArrayMethod));
                                i += 7;
                            }
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}