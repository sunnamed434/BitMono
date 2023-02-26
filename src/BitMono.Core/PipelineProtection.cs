namespace BitMono.Core;

public abstract class PipelineProtection : ProtectionBase, IPipelineProtection
{
    protected PipelineProtection(ProtectionContext context) : base(context)
    {
    }

    public abstract IEnumerable<IPhaseProtection> PopulatePipeline();
}