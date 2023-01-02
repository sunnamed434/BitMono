using BitMono.Core.Extensions;

namespace BitMono.Obfuscation;

public class ProtectionsSorter
{
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly AssemblyDefinition m_Assembly;
    private readonly ILogger m_Logger;

    public ProtectionsSorter(
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        AssemblyDefinition assembly,
        ILogger logger)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_Assembly = assembly;
        m_Logger = logger.ForContext<ProtectionsSorter>();
    }

    public ProtectionsSort Sort(List<IProtection> protections, IEnumerable<ProtectionSettings> protectionSettings)
    {
        var protectionsResolve = new ProtectionsResolver(protections, protectionSettings, m_Logger).Sort();
        var obfuscationAttributeProtections = protectionsResolve.FoundProtections.Where(p => m_ObfuscationAttributeResolver.Resolve(p.GetName(), m_Assembly) == true);
        var deprecatedProtections = protectionsResolve.FoundProtections.Where(p => p.GetType().GetCustomAttribute<ObsoleteAttribute>(false) != null);
        var sortedProtections = protectionsResolve.FoundProtections
            .Except(obfuscationAttributeProtections)
            .Except(deprecatedProtections);

        var packers = sortedProtections.Where(p => p is IPacker)
            .Cast<IPacker>();
        var pipelineProtections = sortedProtections.Where(p => p is IHasPipeline)
            .Cast<IHasPipeline>();

        sortedProtections = sortedProtections
            .Except(packers);

        var hasProtections = sortedProtections.Any();
        return new ProtectionsSort
        {
            FoundProtections = protectionsResolve.FoundProtections,
            SortedProtections = sortedProtections,
            Packers = packers,
            DeprecatedProtections = deprecatedProtections,
            DisabledProtections = protectionsResolve.DisabledProtections,
            Pipelines = pipelineProtections,
            ObfuscationAttributeExcludeProtections = obfuscationAttributeProtections,
            HasProtections = hasProtections
        };
    }
}