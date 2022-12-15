namespace BitMono.Protections;

[ProtectionName(nameof(FullRenamer))]
public class FullRenamer : IProtection
{
    private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
    private readonly TypeDefModelCriticalAnalyzer m_TypeDefModelCriticalAnalyzer;
    private readonly IRenamer m_Renamer;
    private readonly ILogger m_Logger;

    public FullRenamer(
        DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
        TypeDefModelCriticalAnalyzer typeDefModelCriticalAnalyzer,
        IRenamer renamer,
        ILogger logger)
    {
        m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
        m_TypeDefModelCriticalAnalyzer = typeDefModelCriticalAnalyzer;
        m_Renamer = renamer;
        m_Logger = logger.ForContext<FullRenamer>();
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
        {
            if (typeDef.IsGlobalModuleType == false
                && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(typeDef)
                && m_TypeDefModelCriticalAnalyzer.NotCriticalToMakeChanges(typeDef))
            {
                m_Renamer.Rename(typeDef);

                foreach (var fieldDef in typeDef.Fields.ToArray())
                {
                    if (m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(fieldDef))
                    {
                        m_Renamer.Rename(fieldDef);
                    }
                }

                foreach (var methodDef in typeDef.Methods.ToArray())
                {
                    if (methodDef.IsConstructor == false
                        && methodDef.IsVirtual == false
                        && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef))
                    {
                        m_Renamer.Rename(methodDef);
                        if (methodDef.HasParameters())
                        {
                            foreach (var parameter in methodDef.Parameters.ToArray())
                            {
                                m_Renamer.Rename(parameter);
                            }
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}