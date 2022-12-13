using BitMono.API.Protecting.Pipeline;
using BitMono.Utilities.Extensions;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Obfuscation
{
    public class ProtectionsExecutionNotifier
    {
        private readonly ILogger m_Logger;

        public ProtectionsExecutionNotifier(ILogger logger)
        {
            m_Logger = logger.ForContext<ProtectionsExecutionNotifier>();
        }

        public Task NotifyAsync(ProtectionsSortingResult protectionsSortingResult)
        {
            if (protectionsSortingResult.DeprecatedProtections.Any())
            {
                m_Logger.Warning("Deprecated protections which shouldn`t be used anymore: {0}", string.Join(", ", protectionsSortingResult.DeprecatedProtections.Select(p => p?.GetName())));
            }
            if (protectionsSortingResult.Skipped.Any())
            {
                m_Logger.Warning("Skip protections: {0}", string.Join(", ", protectionsSortingResult.Skipped.Select(p => p ?? "NULL")));
            }
            if (protectionsSortingResult.ObfuscationAttributeExcludingProtections.Any())
            {
                m_Logger.Warning("Skip protections with obfuscation attribute excluding: {0}", string.Join(", ", protectionsSortingResult.ObfuscationAttributeExcludingProtections.Select(p => p?.GetName())));
            }
            if (protectionsSortingResult.Protections.Any())
            {
                m_Logger.Information("Execute protections: {0}", string.Join(", ", protectionsSortingResult.Protections.Select(p => p?.GetName())));
            }
            if (protectionsSortingResult.StageProtections.Any())
            {
                var moduleWriteStageProtections = protectionsSortingResult.StageProtections.Where(s => s.Stage == PipelineStages.ModuleWrite);
                if (moduleWriteStageProtections.Any())
                {
                    m_Logger.Information("Execute only when module is writing: {0}", string.Join(", ", moduleWriteStageProtections.Select(p => p?.GetName())));
                }
                var moduleWrittenStageProtections = protectionsSortingResult.StageProtections.Where(s => s.Stage == PipelineStages.ModuleWritten);
                if (moduleWrittenStageProtections.Any())
                {
                    m_Logger.Information("Execute only at the end: {0}", string.Join(", ", moduleWrittenStageProtections.Select(p => p?.GetName())));
                }
            }
            if (protectionsSortingResult.Packers.Any())
            {
                m_Logger.Information("Execute packers: {0}", string.Join(", ", protectionsSortingResult.Packers.Select(p => p?.GetName())));
            }
            return Task.CompletedTask;
        }
    }
}