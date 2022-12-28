namespace BitMono.Obfuscation;

public class ProtectionsExecutionNotifier
{
    private readonly ILogger m_Logger;

    public ProtectionsExecutionNotifier(ILogger logger)
    {
        m_Logger = logger.ForContext<ProtectionsExecutionNotifier>();
    }

    public void Notify(ProtectionsSort protectionsSort)
    {
        if (protectionsSort.DeprecatedProtections.Any())
        {
            m_Logger.Warning("Skip deprecated protections: {0}", string.Join(", ", protectionsSort.DeprecatedProtections.Select(p => p?.GetName())));
        }
        if (protectionsSort.DisabledProtections.Any())
        {
            m_Logger.Warning("Skip protections: {0}", string.Join(", ", protectionsSort.DisabledProtections.Select(p => p ?? "Unknown Protection")));
        }
        if (protectionsSort.ObfuscationAttributeExcludingProtections.Any())
        {
            m_Logger.Warning("Skip protections with obfuscation attribute: {0}", string.Join(", ", protectionsSort.ObfuscationAttributeExcludingProtections.Select(p => p?.GetName())));
        }
        if (protectionsSort.SortedProtections.Any())
        {
            m_Logger.Information("Execute protections: {0}", string.Join(", ", protectionsSort.SortedProtections.Select(p => p?.GetName())));
        }
        if (protectionsSort.StageProtections.Any())
        {
            var moduleWrittenStageProtections = protectionsSort.StageProtections.Where(s => s.Stage == PipelineStages.ModuleWrite);
            if (moduleWrittenStageProtections.Any())
            {
                m_Logger.Information("Execute only at the end: {0}", string.Join(", ", moduleWrittenStageProtections.Select(p => p?.GetName())));
            }
        }
        if (protectionsSort.Packers.Any())
        {
            m_Logger.Information("Execute packers: {0}", string.Join(", ", protectionsSort.Packers.Select(p => p?.GetName())));
        }
    }
}