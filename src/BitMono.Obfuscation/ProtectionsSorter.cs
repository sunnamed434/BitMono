namespace BitMono.Obfuscation;

public class ProtectionsSorter
{
    private readonly IObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly AssemblyDefinition m_Assembly;
    private readonly ILogger m_Logger;

    public ProtectionsSorter(
        IObfuscationAttributeResolver obfuscationAttributeResolver,
        AssemblyDefinition assembly,
        ILogger logger)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_Assembly = assembly;
        m_Logger = logger.ForContext<ProtectionsSorter>();
    }

    public ProtectionsSortResult Sort(List<IProtection> protections, IEnumerable<ProtectionSettings> protectionSettings)
    {
        var protectionsResolveResult = new ProtectionsResolver(protections, protectionSettings, m_Logger).Sort();
        protections = protectionsResolveResult.FoundProtections;

        var packers = protections.Where(p => p is IPacker).Cast<IPacker>().ToList();
        var deprecatedProtections = protections.Where(p => p.GetType().GetCustomAttribute<ObsoleteAttribute>(false) != null);
        var stageProtections = protections.Where(p => p is IStageProtection).Cast<IStageProtection>();
        var pipelineProtections = protections.Where(p => p is IPipelineProtection).Cast<IPipelineProtection>();
        var obfuscationAttributeProtections = protections.Where(p =>
            m_ObfuscationAttributeResolver.Resolve(p.GetName(), m_Assembly));

        protections = protections.Except(obfuscationAttributeProtections).Except(packers).ToList();
        var hasProtections = protections.Any();
        return new ProtectionsSortResult
        {
            Protections = protections,
            Packers = packers,
            DeprecatedProtections = deprecatedProtections,
            DisabledProtections = protectionsResolveResult.DisabledProtections,
            StageProtections = stageProtections,
            PipelineProtections = pipelineProtections,
            ObfuscationAttributeExcludingProtections = obfuscationAttributeProtections,
            HasProtections = hasProtections
        };
    }
}