using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Resolvers;
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
        private readonly IObfuscationAttributeExcludingResolver m_ObfuscationAttributeExcludingResolver;
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;

        public ObjectReturnType(
            IObfuscationAttributeExcludingResolver obfuscationAttributeExcludingResolver,
            DnlibDefCriticalAnalyzer methodDefCriticalAnalyzer,
            ILogger logger)
        {
            m_ObfuscationAttributeExcludingResolver = obfuscationAttributeExcludingResolver;
            m_DnlibDefCriticalAnalyzer = methodDefCriticalAnalyzer;
            m_Logger = logger.ForContext<ObjectReturnType>();
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (m_ObfuscationAttributeExcludingResolver.TryResolve(typeDef, feature: nameof(ObjectReturnType),
                    out ObfuscationAttribute typeDefObfuscationAttribute))
                {
                    if (typeDefObfuscationAttribute.Exclude)
                    {
                        m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                        continue;
                    }
                }
                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods.ToArray())
                    {
                        if (m_ObfuscationAttributeExcludingResolver.TryResolve(typeDef, feature: nameof(ObjectReturnType),
                            out ObfuscationAttribute methodDefObfuscationAttribute))
                        {
                            if (methodDefObfuscationAttribute.Exclude)
                            {
                                m_Logger.Debug("Found {0}, skipping.", nameof(ObfuscationAttribute));
                                continue;
                            }
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