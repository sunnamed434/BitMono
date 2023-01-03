namespace BitMono.API.Protecting.Pipeline;

public interface IPipelineProtection : IProtection
{
    IEnumerable<IPhaseProtection> PopulatePipeline();
}