namespace BitMono.Protections;

public class NoNamespaces : IProtection
{
    private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;

    public NoNamespaces(DnlibDefCriticalAnalyzer typeDefCriticalAnalyzer)
    {
        m_DnlibDefCriticalAnalyzer = typeDefCriticalAnalyzer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        foreach (var typeDef in parameters.Targets.OfType<TypeDefinition>())
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