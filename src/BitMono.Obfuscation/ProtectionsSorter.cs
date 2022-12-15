namespace BitMono.Obfuscation;

public class ProtectionsSorter
{
    private readonly IDnlibDefObfuscationAttributeResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
    private readonly AssemblyDef m_ModuleDefMDAssemblyDef;
    private readonly ILogger m_Logger;

    public ProtectionsSorter(
        IDnlibDefObfuscationAttributeResolver dnlibDefFeatureObfuscationAttributeHavingResolver,
        AssemblyDef moduleDefMDAssemblyDef,
        ILogger logger)
    {
        m_DnlibDefFeatureObfuscationAttributeHavingResolver = dnlibDefFeatureObfuscationAttributeHavingResolver;
        m_ModuleDefMDAssemblyDef = moduleDefMDAssemblyDef;
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
        var obfuscationAttributeExcludingProtections = protections.Where(p =>
            m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve(p.GetName(), m_ModuleDefMDAssemblyDef));

        protections = protections.Except(obfuscationAttributeExcludingProtections).ToList();
        var hasProtections = protections.Any();
        return new ProtectionsSortResult
        {
            Protections = protections,
            Packers = packers,
            DeprecatedProtections = deprecatedProtections,
            DisabledProtections = protectionsResolveResult.DisabledProtections,
            StageProtections = stageProtections,
            PipelineProtections = pipelineProtections,
            ObfuscationAttributeExcludingProtections = obfuscationAttributeExcludingProtections,
            HasProtections = hasProtections
        };
    }
}