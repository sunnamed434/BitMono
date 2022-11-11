using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Core.Protecting.Modules
{
    public class ProtectionsExecutionNotifier
    {
        private readonly ILogger m_Logger;

        public ProtectionsExecutionNotifier(ILogger logger)
        {
            m_Logger = logger;
        }

        public Task NotifyAsync(ProtectionsSortingResult protectionsSortingResult)
        {
            if (protectionsSortingResult.Skipped.Any())
            {
                m_Logger.Warning("Skip protections: {0}", string.Join(", ", protectionsSortingResult.Skipped.Select(p => p ?? "NULL")));
            }
            if (protectionsSortingResult.ObfuscationAttributeExcludingProtections.Any())
            {
                m_Logger.Warning("Skip protections with obfuscation attribute excluding: {0}", string.Join(", ", protectionsSortingResult.ObfuscationAttributeExcludingProtections.Select(p => p.GetType().Name ?? "NULL")));
            }
            if (protectionsSortingResult.Protections.Any())
            {
                m_Logger.Information("Execute protections: {0}", string.Join(", ", protectionsSortingResult.Protections.Select(p => p.GetType().Name ?? "NULL")));
            }
            if (protectionsSortingResult.StageProtections.Any())
            {
                m_Logger.Information("Execute calling condition protections: {0}", string.Join(", ", protectionsSortingResult.StageProtections.Select(p => p.GetType().Name ?? "NULL")));
            }
            return Task.CompletedTask;
        }
    }
}