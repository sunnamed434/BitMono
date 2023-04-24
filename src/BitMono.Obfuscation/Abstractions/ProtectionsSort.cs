namespace BitMono.Obfuscation.Abstractions;

public class ProtectionsSort
{
#pragma warning disable CS8618
    public ProtectionsResolve ProtectionsResolve { get; set; }
    public IEnumerable<IProtection> SortedProtections { get; set; }
    public IEnumerable<IPipelineProtection> Pipelines { get; set; }
    public IEnumerable<IPacker> Packers { get; set; }
    public IEnumerable<IProtection> DeprecatedProtections { get; set; }
    public IEnumerable<IProtection> ObfuscationAttributeExcludeProtections { get; set; }
    public bool HasProtections { get; set; }
#pragma warning restore CS8618
}