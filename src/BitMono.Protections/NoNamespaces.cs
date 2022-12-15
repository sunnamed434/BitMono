namespace BitMono.Protections;

[ProtectionName(nameof(NoNamespaces))]
public class NoNamespaces : IProtection
{
    private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
    private readonly ILogger m_Logger;

    public NoNamespaces(
        DnlibDefCriticalAnalyzer typeDefCriticalAnalyzer, 
        ILogger logger)
    {
        m_DnlibDefCriticalAnalyzer = typeDefCriticalAnalyzer;
        m_Logger = logger.ForContext<NoNamespaces>();
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
        {
            if (typeDef.IsGlobalModuleType == false
                && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(typeDef)
                && typeDef.HasNamespace())
            {
                typeDef.Namespace = string.Empty;
            }
        }
        return Task.CompletedTask;
    }
}