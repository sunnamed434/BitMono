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
                m_Logger.Warning("Deprecated protections which shouldn`t be used anymore: {0}", string.Join(", ", protectionsSortingResult.DeprecatedProtections.Select(p => p?.GetType()?.Name ?? "NULL")));
            }
            if (protectionsSortingResult.Skipped.Any())
            {
                m_Logger.Warning("Skip protections: {0}", string.Join(", ", protectionsSortingResult.Skipped.Select(p => p ?? "NULL")));
            }
            if (protectionsSortingResult.ObfuscationAttributeExcludingProtections.Any())
            {
                m_Logger.Warning("Skip protections with obfuscation attribute excluding: {0}", string.Join(", ", protectionsSortingResult.ObfuscationAttributeExcludingProtections.Select(p => p?.GetType()?.Name ?? "NULL")));
            }
            if (protectionsSortingResult.Protections.Any())
            {
                m_Logger.Information("Execute protections: {0}", string.Join(", ", protectionsSortingResult.Protections.Select(p => p?.GetType()?.Name ?? "NULL")));
            }
            if (protectionsSortingResult.StageProtections.Any())
            {
                m_Logger.Information("Execute only at the end: {0}", string.Join(", ", protectionsSortingResult.StageProtections.Select(p => p?.GetType()?.Name ?? "NULL")));
            }
            return Task.CompletedTask;
        }
    }
}