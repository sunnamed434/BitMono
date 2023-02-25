namespace BitMono.Protections;

[RuntimeMonikerMono]
public class AntiDecompiler : PipelineProtection
{
    public AntiDecompiler(ProtectionContext context) : base(context)
    {
    }

    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        return Task.CompletedTask;
    }
    public override IEnumerable<IPhaseProtection> PopulatePipeline()
    {
        yield return new AntiDnSpyAnalyzer();
    }
}
[ProtectionName(nameof(AntiDnSpyAnalyzer))]
public class AntiDnSpyAnalyzer : IPhaseProtection
{
    public Task ExecuteAsync(ProtectionParameters parameters)
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