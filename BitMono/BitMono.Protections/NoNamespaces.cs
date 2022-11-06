using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Analyzing.Naming;
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
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;

        public NoNamespaces(
            IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver, 
            DnlibDefCriticalAnalyzer typeDefCriticalAnalyzer, 
            ILogger logger)
        {
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
            m_DnlibDefCriticalAnalyzer = typeDefCriticalAnalyzer;
            m_Logger = logger.ForContext<NoNamespaces>();
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_ObfuscationAttributeExcludingResolver.TryResolve(typeDef, feature: nameof(NoNamespaces),
                    out ObfuscationAttribute typeDefObfuscationAttribute))
                {
                    if (typeDefObfuscationAttribute.Exclude)
                    {
                        m_Logger.Information("Found {0}, skipping.", nameof(ObfuscationAttribute));
                        continue;
                    }
                }

                if (typeDef.IsGlobalModuleType == false
                    && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef)
                    && UTF8String.IsNullOrEmpty(typeDef.Namespace) == false)
                {
                    typeDef.Namespace = string.Empty;
                }
            }
            return Task.CompletedTask;
        }
    }
}