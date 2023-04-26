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
        var protectionsResolve = new ProtectionsResolver(protections, protectionSettings)
            .Sort();
        var obfuscationAttributeProtections =
            protectionsResolve.FoundProtections.Where(p =>
                _obfuscationAttributeResolver.Resolve(p.GetName(), _assemblyDefinition));
        var deprecatedProtections =
            protectionsResolve.FoundProtections.Where(p => p.TryGetObsoleteAttribute(out _));
        var sortedProtections = protectionsResolve.FoundProtections
            .Except(obfuscationAttributeProtections)
            .Except(deprecatedProtections);
        var pipelineProtections = sortedProtections
            .Where(p => p is IPipelineProtection)
            .Cast<IPipelineProtection>();
        var packers = sortedProtections
            .Where(p => p is IPacker)
            .Cast<IPacker>();
        sortedProtections = sortedProtections
            .Except(packers)
            .Except(pipelineProtections);

        var hasProtections = sortedProtections.IsEmpty() == false || packers.IsEmpty() == false;
        return new ProtectionsSort
        {
            ProtectionsResolve = protectionsResolve,
            SortedProtections = sortedProtections,
            Pipelines = pipelineProtections,
            Packers = packers,
            ObfuscationAttributeExcludeProtections = obfuscationAttributeProtections,
            DeprecatedProtections = deprecatedProtections,
            HasProtections = hasProtections
        };
    }
}