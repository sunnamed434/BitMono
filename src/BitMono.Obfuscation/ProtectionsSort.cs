namespace BitMono.Obfuscation;

public class ProtectionsSort
{
    public ICollection<IProtection> FoundProtections { get; set; }
    public ICollection<IProtection> SortedProtections { get; set; }
    public ICollection<IPacker> Packers { get; set; }
    public IEnumerable<IProtection> DeprecatedProtections { get; set; }
    public ICollection<string> DisabledProtections { get; set; }
    public IEnumerable<IStageProtection> StageProtections { get; set; }
    public IEnumerable<IPipelineProtection> PipelineProtections { get; set; }
    public IEnumerable<IProtection> ObfuscationAttributeExcludingProtections { get; set; }
    public bool HasProtections { get; set; }
}