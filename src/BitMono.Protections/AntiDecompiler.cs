namespace BitMono.Protections;

[RuntimeMonikerMono]
public class AntiDecompiler : IPipelineProtection
{
    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        return Task.CompletedTask;
    }
    public IEnumerable<IPhaseProtection> PopulatePipeline()
    {
        yield return new AntiDnSpyAnalyzer();
    }
}
[ProtectionName(nameof(AntiDnSpyAnalyzer))]
public class AntiDnSpyAnalyzer : IPhaseProtection
{
    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        foreach (var type in parameters.Members.OfType<TypeDefinition>())
        {
            if (type.IsModuleType && type.IsNested)
            {
                type.Attributes = TypeAttributes.Sealed | TypeAttributes.ExplicitLayout;
            }
        }
        return Task.CompletedTask;
    }
}