using BitMono.Core.Extensions.Protections;

namespace BitMono.Obfuscation;

public class ProtectionsExecutionNotifier
{
    private readonly ILogger m_Logger;

    public ProtectionsExecutionNotifier(ILogger logger)
    {
        m_Logger = logger.ForContext<ProtectionsExecutionNotifier>();
    }

    public void Notify(ProtectionsSortResult protectionsSortResult)
    {
        if (protectionsSortResult.DeprecatedProtections.Any())
        {
            m_Logger.Warning("Deprecated protections which are deprecated: {0}", string.Join(", ", protectionsSortResult.DeprecatedProtections.Select(p => p?.GetName())));
        }
        if (protectionsSortResult.DisabledProtections.Any())
        {
            m_Logger.Warning("Skip protections: {0}", string.Join(", ", protectionsSortResult.DisabledProtections.Select(p => p ?? "NULL")));
        }
        if (protectionsSortResult.ObfuscationAttributeExcludingProtections.Any())
        {
            m_Logger.Warning("Skip protections with obfuscation attribute excluding: {0}", string.Join(", ", protectionsSortResult.ObfuscationAttributeExcludingProtections.Select(p => p?.GetName())));
        }
        if (protectionsSortResult.Protections.Any())
        {
            m_Logger.Information("Execute protections: {0}", string.Join(", ", protectionsSortResult.Protections.Select(p => p?.GetName())));
        }
        if (protectionsSortResult.StageProtections.Any())
        {
            var moduleWrittenStageProtections = protectionsSortResult.StageProtections.Where(s => s.Stage == PipelineStages.ModuleWritten);
            if (moduleWrittenStageProtections.Any())
            {
                m_Logger.Information("Execute only at the end: {0}", string.Join(", ", moduleWrittenStageProtections.Select(p => p?.GetName())));
            }
        }
        if (protectionsSortResult.Packers.Any())
        {
            m_Logger.Information("Execute packers: {0}", string.Join(", ", protectionsSortResult.Packers.Select(p => p?.GetName())));
        }
    }
}