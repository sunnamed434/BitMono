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
        foreach (var type in parameters.Targets.OfType<TypeDefinition>())
        {
            if (type.HasNamespace() && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                type.Namespace = string.Empty;
            }
        }
        return Task.CompletedTask;
    }
}