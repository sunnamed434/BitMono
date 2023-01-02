namespace BitMono.Obfuscation;

public class ProtectionsSort
{
    public IEnumerable<IProtection> FoundProtections { get; set; }
    public IEnumerable<IProtection> SortedProtections { get; set; }
    public IEnumerable<IPacker> Packers { get; set; }
    public IEnumerable<IProtection> DeprecatedProtections { get; set; }
    public IEnumerable<string> DisabledProtections { get; set; }
    public IEnumerable<IHasPipeline> Pipelines { get; set; }
    public IEnumerable<IProtection> ObfuscationAttributeExcludeProtections { get; set; }
    public bool HasProtections { get; set; }
}