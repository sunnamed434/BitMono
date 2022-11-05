using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
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
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly TypeDefModelCriticalAnalyzer m_TypeDefModelCriticalAnalyzer;
        private readonly IRenamer m_Renamer;
        private readonly ILogger m_Logger;

        public FullRenamer(
            IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver,
            DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer,
            TypeDefModelCriticalAnalyzer typeDefModelCriticalAnalyzer,
            IRenamer renamer,
            ILogger logger)
        {
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
            m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
            m_TypeDefModelCriticalAnalyzer = typeDefModelCriticalAnalyzer;
            m_Renamer = renamer;
            m_Logger = logger.ForContext<FullRenamer>();
        }


        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_ObfuscationAttributeExcludingResolver.TryResolve(typeDef, feature: nameof(FullRenamer),
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
                    && m_TypeDefModelCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef))
                {
                    m_Renamer.Rename(context, typeDef);

                    if (typeDef.HasFields)
                    {
                        foreach (var fieldDef in typeDef.Fields.ToArray())
                        {
                            if (m_ObfuscationAttributeExcludingResolver.TryResolve(fieldDef, feature: nameof(FullRenamer),
                                out ObfuscationAttribute fieldDefObfuscationAttribute))
                            {
                                if (fieldDefObfuscationAttribute.Exclude)
                                {
                                    m_Logger.Information("Found {0}, skipping.", nameof(ObfuscationAttribute));
                                    continue;
                                }
                            }

                            if (m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(context, fieldDef))
                            {
                                m_Renamer.Rename(context, fieldDef);
                            }
                        }
                    }

                    if (typeDef.HasMethods)
                    {
                        foreach (var methodDef in typeDef.Methods.ToArray())
                        {
                            if (m_ObfuscationAttributeExcludingResolver.TryResolve(methodDef, feature: nameof(FullRenamer),
                                out ObfuscationAttribute methodDefObfuscationAttribute))
                            {
                                if (methodDefObfuscationAttribute.Exclude)
                                {
                                    m_Logger.Information("Found {0}, skipping.", nameof(ObfuscationAttribute));
                                    continue;
                                }
                            }

                            if (methodDef.IsConstructor == false
                                && methodDef.IsVirtual == false
                                && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(context, methodDef))
                            {
                                m_Renamer.Rename(context, methodDef);
                                if (methodDef.HasParameters())
                                {
                                    foreach (var parameter in methodDef.Parameters.ToArray())
                                    {
                                        m_Renamer.Rename(context, parameter);
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