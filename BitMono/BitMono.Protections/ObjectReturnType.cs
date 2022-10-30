using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Utilities.Extensions.dnlib;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class ObjectReturnType : IProtection
    {
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;

        public ObjectReturnType(DnlibDefCriticalAnalyzer methodDefCriticalAnalyzer)
        {
            m_DnlibDefCriticalAnalyzer = methodDefCriticalAnalyzer;
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods.ToArray())
                    {
                        if (methodDef.HasReturnType
                            && methodDef.ReturnType != context.ModuleDefMD.CorLibTypes.Boolean)
                        {
                            if (methodDef.IsConstructor == false && methodDef.IsVirtual == false
                                && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(context, methodDef)
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