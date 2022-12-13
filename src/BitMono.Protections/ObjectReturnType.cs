using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.Core.Protecting;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Attributes;
using BitMono.Utilities.Extensions.dnlib;
using dnlib.DotNet;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.Protections
{
    [ProtectionName(nameof(ObjectReturnType))]
    public class ObjectReturnType : IProtection
    {
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly ILogger m_Logger;

        public ObjectReturnType(
            DnlibDefCriticalAnalyzer methodDefCriticalAnalyzer,
            ILogger logger)
        {
            m_DnlibDefCriticalAnalyzer = methodDefCriticalAnalyzer;
            m_Logger = logger.ForContext<ObjectReturnType>();
        }

        public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in parameters.Targets.OfType<TypeDef>())
            {
                foreach (var methodDef in typeDef.Methods.ToArray())
                {
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
            return Task.CompletedTask;
        }
    }
}