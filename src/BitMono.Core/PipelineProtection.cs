namespace BitMono.Core;

public abstract class PipelineProtection : ProtectionBase, IPipelineProtection
{
    protected PipelineProtection(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public abstract IEnumerable<IPhaseProtection> PopulatePipeline();
}