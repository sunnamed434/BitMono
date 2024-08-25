namespace BitMono.Obfuscation.Protections;

public class ProtectionsSort
{
    public ProtectionsSort(
        ProtectionsResolve protectionsResolve,
        IReadOnlyCollection<IProtection> allProtections,
        IReadOnlyCollection<IProtection> sortedProtections,
        IReadOnlyCollection<IPipelineProtection> pipelines,
        IReadOnlyCollection<IPacker> packers,
        IReadOnlyCollection<IProtection> deprecatedProtections,
        IReadOnlyCollection<IProtection> obfuscationAttributeExcludeProtections,
        IReadOnlyCollection<IProtection> configureForNativeCodeProtections,
        IReadOnlyCollection<(IProtection, RuntimeMonikerAttribute[])> runtimeMonikerProtections,
        bool hasProtections)
    {
        ProtectionsResolve = protectionsResolve;
        AllProtections = allProtections;
        SortedProtections = sortedProtections;
        Pipelines = pipelines;
        Packers = packers;
        DeprecatedProtections = deprecatedProtections;
        ObfuscationAttributeExcludeProtections = obfuscationAttributeExcludeProtections;
        ConfigureForNativeCodeProtections = configureForNativeCodeProtections;
        RuntimeMonikerProtections = runtimeMonikerProtections;
        HasProtections = hasProtections;
    }

#pragma warning disable CS8618
    public ProtectionsResolve ProtectionsResolve { get; }
    /// <summary>
    /// Gets a collection of <see cref="IProtection"/>, <see cref="IPipelineProtection"/>, and <see cref="IPacker"/>.
    /// </summary>
    public IReadOnlyCollection<IProtection> AllProtections { get; }
    public IReadOnlyCollection<IProtection> SortedProtections { get; }
    public IReadOnlyCollection<IPipelineProtection> Pipelines { get; }
    public IReadOnlyCollection<IPacker> Packers { get; }
    public IReadOnlyCollection<IProtection> DeprecatedProtections { get; }
    public IReadOnlyCollection<IProtection> ObfuscationAttributeExcludeProtections { get; }
    public IReadOnlyCollection<IProtection> ConfigureForNativeCodeProtections { get; }
    public IReadOnlyCollection<(IProtection, RuntimeMonikerAttribute[])> RuntimeMonikerProtections { get; }
    public bool HasProtections { get; }
#pragma warning restore CS8618
}