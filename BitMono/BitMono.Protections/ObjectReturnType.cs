using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Utilities.Extensions.dnlib;
using Serilog;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class ObjectReturnType : IProtection
    {
        private readonly DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer<ObjectReturnType> m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer;
        private readonly DnlibDefSpecificNamespaceHavingCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;

        public ObjectReturnType(
            DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer<ObjectReturnType> dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer,
            DnlibDefSpecificNamespaceHavingCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            DnlibDefCriticalAnalyzer methodDefCriticalAnalyzer,
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer = dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_DnlibDefCriticalAnalyzer = methodDefCriticalAnalyzer;
            m_Logger = logger.ForContext<ObjectReturnType>();
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

                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods.ToArray())
                    {
                        if (m_DnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer.NotCriticalToMakeChanges(methodDef) == false)
                        {
                            m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                            continue;
                        }

                        if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(methodDef) == false)
                        {
                            m_Logger.Debug("Not able to make changes because of specific namespace was found, skipping.");
                            continue;
                        }

                        if (methodDef.HasReturnType
                            && methodDef.ReturnType != context.ModuleDefMD.CorLibTypes.Boolean)
                        {
                            if (methodDef.IsConstructor == false && methodDef.IsVirtual == false
                                && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef)
                                && methodDef.NotAsync())
                            {
                                if (methodDef.IsSetter == false && methodDef.IsGetter == false)
                                {
                                    if (methodDef.Parameters.Any(p => p.HasParamDef && (p.ParamDef.IsOut || p.ParamDef.IsIn)) == false)
                                    {
                                        methodDef.ReturnType = context.ModuleDefMD.CorLibTypes.Object;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}