namespace BitMono.Protections;

public class NoNamespaces : IProtection
{
    private readonly CriticalAnalyzer m_DnlibDefCriticalAnalyzer;

    public NoNamespaces(CriticalAnalyzer typeDefCriticalAnalyzer)
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