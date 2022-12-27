using BitMono.Core.Protecting.Analyzing;

namespace BitMono.Protections;

public class NoNamespaces : IProtection
{
    private readonly RuntimeCriticalAnalyzer m_RuntimeCriticalAnalyzer;

    public NoNamespaces(RuntimeCriticalAnalyzer runtimeCriticalAnalyzer)
    {
        m_RuntimeCriticalAnalyzer = runtimeCriticalAnalyzer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        foreach (var type in parameters.Targets.OfType<TypeDefinition>())
        {
            if (type.HasNamespace() && m_RuntimeCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                type.Namespace = string.Empty;
            }
        }
        return Task.CompletedTask;
    }
}