using BitMono.API.Protecting;
using BitMono.API.Protecting.Pipeline;
using System.Collections.Generic;

namespace BitMono.Obfuscation
{
    public class ProtectionsSortingResult
    {
        public ICollection<IProtection> Protections { get; set; }
        public IEnumerable<IProtection> DeprecatedProtections { get; set; }
        public ICollection<string> Skipped { get; set; }
        public IEnumerable<IStageProtection> StageProtections { get; set; }
        public IEnumerable<IPipelineProtection> PipelineProtections { get; set; }
        public IEnumerable<IProtection> ObfuscationAttributeExcludingProtections { get; set; }
    }
}