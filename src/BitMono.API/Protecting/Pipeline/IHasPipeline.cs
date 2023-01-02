namespace BitMono.API.Protecting.Pipeline;

public interface IHasPipeline
{
    IEnumerable<IPhaseProtection> PopulatePipeline();
}