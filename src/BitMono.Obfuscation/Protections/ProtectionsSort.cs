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
        IReadOnlyCollection<(IProtection Protection, string Reason)> il2cppIncompatibleProtections,
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
        IL2CPPIncompatibleProtections = il2cppIncompatibleProtections;
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
    /// <summary>
    /// Protections skipped because they don't work on IL2CPP builds, with the reason for each.
    /// Only populated when running in IL2CPP mode (see <see cref="ObfuscationSettings.IL2CPP"/>); empty otherwise.
    /// </summary>
    public IReadOnlyCollection<(IProtection Protection, string Reason)> IL2CPPIncompatibleProtections { get; }
    public bool HasProtections { get; }
#pragma warning restore CS8618
}