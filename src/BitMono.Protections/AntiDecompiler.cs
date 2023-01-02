namespace BitMono.Protections;

public class AntiDecompiler : IHasPipeline
{
    public IEnumerable<IPhaseProtection> PopulatePipeline()
    {
        yield return new AntiDnSpyAnalyzer();
    }
}
[ProtectionName(nameof(AntiDnSpyAnalyzer))]
public class AntiDnSpyAnalyzer : IPhaseProtection
{
    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        foreach (var type in parameters.Targets.OfType<TypeDefinition>())
        {
            if (type.IsNested)
            {
                type.Attributes = TypeAttributes.Sealed | TypeAttributes.ExplicitLayout;
            }
        }
        return Task.CompletedTask;
    }
}