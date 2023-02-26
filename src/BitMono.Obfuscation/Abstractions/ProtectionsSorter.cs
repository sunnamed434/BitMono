namespace BitMono.Obfuscation.Abstractions;

public class ProtectionsSorter
{
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly AssemblyDefinition m_Assembly;

    public ProtectionsSorter(ObfuscationAttributeResolver obfuscationAttributeResolver, AssemblyDefinition assembly)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_Assembly = assembly;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public ProtectionsSort Sort(List<IProtection> protections, IEnumerable<ProtectionSetting> protectionSettings)
    {
        var protectionsResolve = new ProtectionsResolver(protections, protectionSettings)
            .Sort();
        var obfuscationAttributeProtections =
            protectionsResolve.FoundProtections.Where(p =>
                m_ObfuscationAttributeResolver.Resolve(p.GetName(), m_Assembly));
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