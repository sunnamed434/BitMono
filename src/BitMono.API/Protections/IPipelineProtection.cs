namespace BitMono.API.Protections;

public interface IPipelineProtection : IProtection
{
    IEnumerable<IPhaseProtection> PopulatePipeline();
}