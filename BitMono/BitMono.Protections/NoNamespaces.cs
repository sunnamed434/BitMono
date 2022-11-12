using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Protections
{
    public class NoNamespaces : IProtection
    {
        private readonly DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer<NoNamespaces> m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer;
        private readonly DnlibDefSpecificNamespaceHavingCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;

        public NoNamespaces(
            DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer<NoNamespaces> dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer,
            DnlibDefSpecificNamespaceHavingCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            DnlibDefCriticalAnalyzer typeDefCriticalAnalyzer, 
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer = dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_DnlibDefCriticalAnalyzer = typeDefCriticalAnalyzer;
            m_Logger = logger.ForContext<NoNamespaces>();
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer.NotCriticalToMakeChanges(typeDef) == false)
                {
                    m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                    continue;
                }

                if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(typeDef) == false)
                {
                    m_Logger.Debug("Not able to make changes because of specific namespace was found, skipping.");
                    continue;
                }

                if (typeDef.IsGlobalModuleType == false
                    && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(typeDef)
                    && typeDef.HasNamespace())
                {
                    typeDef.Namespace = string.Empty;
                }
            }
            return Task.CompletedTask;
        }
    }
}