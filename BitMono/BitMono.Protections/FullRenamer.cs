using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Renaming;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Analyzing.TypeDefs;
using BitMono.Utilities.Extensions.dnlib;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Protections
{
    public class FullRenamer : IProtection
    {
        private readonly IDnlibDefFeatureObfuscationAttributeHavingResolver m_DnlibDefFeatureObfuscationAttributeHavingResolver;
        private readonly DnlibDefSpecificNamespaceHavingCriticalAnalyzer m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly TypeDefModelCriticalAnalyzer m_TypeDefModelCriticalAnalyzer;
        private readonly IRenamer m_Renamer;
        private readonly ILogger m_Logger;

        public FullRenamer(
            IDnlibDefFeatureObfuscationAttributeHavingResolver dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer,
            DnlibDefSpecificNamespaceHavingCriticalAnalyzer dnlibDefSpecificNamespaceHavingCriticalAnalyzer,
            DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
            TypeDefModelCriticalAnalyzer typeDefModelCriticalAnalyzer,
            IRenamer renamer,
            ILogger logger)
        {
            m_DnlibDefFeatureObfuscationAttributeHavingResolver = dnlibDefFeatureObfuscationAttributeHavingCriticalAnalyzer;
            m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer = dnlibDefSpecificNamespaceHavingCriticalAnalyzer;
            m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
            m_TypeDefModelCriticalAnalyzer = typeDefModelCriticalAnalyzer;
            m_Renamer = renamer;
            m_Logger = logger.ForContext<FullRenamer>();
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve<FullRenamer>(typeDef) == false)
                {
                    m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                    continue;
                }

                if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(typeDef))
                {
                    m_Logger.Debug("Not able to make changes because of specific namespace was found, skipping.");
                    continue;
                }

                if (typeDef.IsGlobalModuleType == false
                    && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(typeDef)
                    && m_TypeDefModelCriticalAnalyzer.NotCriticalToMakeChanges(typeDef))
                {
                    m_Renamer.Rename(typeDef);

                    if (typeDef.HasFields)
                    {
                        foreach (var fieldDef in typeDef.Fields.ToArray())
                        {
                            if (m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve<FullRenamer>(fieldDef))
                            {
                                m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                                continue;
                            }

                            if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(fieldDef) == false)
                            {
                                m_Logger.Debug("Not able to make changes because of specific namespace was found, skipping.");
                                continue;
                            }

                            if (m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(fieldDef))
                            {
                                m_Renamer.Rename(fieldDef);
                            }
                        }
                    }

                    if (typeDef.HasMethods)
                    {
                        foreach (var methodDef in typeDef.Methods.ToArray())
                        {
                            if (m_DnlibDefFeatureObfuscationAttributeHavingResolver.Resolve<FullRenamer>(methodDef))
                            {
                                m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                                continue;
                            }

                            if (m_DnlibDefSpecificNamespaceHavingCriticalAnalyzer.NotCriticalToMakeChanges(methodDef) == false)
                            {
                                m_Logger.Debug("Not able to make changes because of specific namespace was found, skipping.");
                                continue;
                            }

                            if (methodDef.IsConstructor == false
                                && methodDef.IsVirtual == false
                                && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(methodDef))
                            {
                                m_Renamer.Rename(methodDef);
                                if (methodDef.HasParameters())
                                {
                                    foreach (var parameter in methodDef.Parameters.ToArray())
                                    {
                                        m_Renamer.Rename(parameter);
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