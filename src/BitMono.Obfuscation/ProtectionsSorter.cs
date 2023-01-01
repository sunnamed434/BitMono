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
        protections = protectionsResolve.FoundProtections;
        var obfuscationAttributeProtections = protections.Where(p => m_ObfuscationAttributeResolver.Resolve(p.GetName(), m_Assembly) == true);
        var deprecatedProtections = protections.Where(p => p.GetType().GetCustomAttribute<ObsoleteAttribute>(false) != null);
        var sortedProtections = protections
            .Except(obfuscationAttributeProtections)
            .Except(deprecatedProtections)
            .ToList();

        var packers = sortedProtections.Where(p => p is IPacker)
            .Cast<IPacker>()
            .ToList();
        var stageProtections = sortedProtections.Where(p => p is IStageProtection)
            .Cast<IStageProtection>();
        var pipelineProtections = sortedProtections.Where(p => p is IPipelineProtection)
            .Cast<IPipelineProtection>();

        sortedProtections = sortedProtections
            .Except(packers)
            .ToList();
        var hasProtections = protections.Any();
        return new ProtectionsSort
        {
            FoundProtections = protectionsResolve.FoundProtections,
            SortedProtections = sortedProtections,
            Packers = packers,
            DeprecatedProtections = deprecatedProtections,
            DisabledProtections = protectionsResolve.DisabledProtections,
            StageProtections = stageProtections,
            PipelineProtections = pipelineProtections,
            ObfuscationAttributeExcludeProtections = obfuscationAttributeProtections,
            HasProtections = hasProtections
        };
    }
}