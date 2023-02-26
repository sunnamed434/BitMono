namespace BitMono.API.Pipeline;

public interface IPipelineProtection : IProtection
{
    IEnumerable<IPhaseProtection> PopulatePipeline();
}