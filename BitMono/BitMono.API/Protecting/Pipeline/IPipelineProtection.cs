using System.Collections.Generic;

namespace BitMono.API.Protecting.Pipeline
{
    public interface IPipelineProtection : IProtection
    {
        IEnumerable<(IProtectionPhase, PipelineStages)> PopulatePipeline();
    }
}