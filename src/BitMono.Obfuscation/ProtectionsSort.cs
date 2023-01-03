namespace BitMono.Obfuscation;

public class ProtectionsSort
{
    public ProtectionsResolve ProtectionsResolve { get; set; } 
    public IEnumerable<IProtection> SortedProtections { get; set; }
    public IEnumerable<IPipelineProtection> Pipelines { get; set; }
    public IEnumerable<IPacker> Packers { get; set; }
    public IEnumerable<IProtection> DeprecatedProtections { get; set; }
    public IEnumerable<IProtection> ObfuscationAttributeExcludeProtections { get; set; }
    public bool HasProtections { get; set; }
}