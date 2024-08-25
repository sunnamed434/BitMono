namespace BitMono.Obfuscation.Protections;

public class ProtectionsSorter
{
    private readonly ObfuscationAttributeResolver _obfuscationAttributeResolver;
    private readonly AssemblyDefinition _assemblyDefinition;

    public ProtectionsSorter(ObfuscationAttributeResolver obfuscationAttributeResolver, AssemblyDefinition assemblyDefinition)
    {
        _obfuscationAttributeResolver = obfuscationAttributeResolver;
        _assemblyDefinition = assemblyDefinition;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public ProtectionsSort Sort(List<IProtection> protections, IEnumerable<ProtectionSetting> protectionSettings)
    {
        var protectionsResolve = new ProtectionsResolver(protections, protectionSettings).Sort();
        var obfuscationAttributeProtections =
            protectionsResolve.FoundProtections.Where(x =>
                _obfuscationAttributeResolver.Resolve(x.GetName(), _assemblyDefinition));
        var deprecatedProtections =
            protectionsResolve.FoundProtections.Where(x => x.TryGetObsoleteAttribute(out _));
        var sortedProtections = protectionsResolve.FoundProtections
            .Except(obfuscationAttributeProtections)
            .Except(deprecatedProtections);
        var pipelineProtections = sortedProtections
            .Where(x => x is IPipelineProtection)
            .Cast<IPipelineProtection>();
        var packers = sortedProtections
            .Where(x => x is IPacker)
            .Cast<IPacker>();
        sortedProtections = sortedProtections
            .Except(packers)
            .Except(pipelineProtections);
        var allProtections = sortedProtections.Concat(pipelineProtections).Concat(packers);

        var configureForNativeCodeProtections = allProtections.Where(
            x => x.GetConfigureForNativeCodeAttribute() != null);
        var runtimeMonikerProtections = allProtections
            .Select(x => (x, x.GetRuntimeMonikerAttributes()))
            .Where(x => x.Item2.Any());

        var hasProtections = sortedProtections.IsEmpty() == false || packers.IsEmpty() == false;

        return new ProtectionsSort(
            protectionsResolve,
            allProtections.ToList(),
            sortedProtections.ToList(),
            pipelineProtections.ToList(),
            packers.ToList(),
            obfuscationAttributeProtections.ToList(),
            deprecatedProtections.ToList(),
            configureForNativeCodeProtections.ToList(),
            runtimeMonikerProtections.ToList(),
            hasProtections);
    }
}