using BitMono.API.Protections;
using BitMono.Core;
using BitMono.Core.Analyzing;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class ObjectReturnType : IProtection
    {
        private readonly MethodDefCriticalAnalyzer m_MethodDefCriticalAnalyzer;

        public ObjectReturnType(MethodDefCriticalAnalyzer methodDefCriticalAnalyzer, ILogger logger)
        {
            m_MethodDefCriticalAnalyzer = methodDefCriticalAnalyzer;
            logger.Warn(this, "Hello world!");
            logger.Debug(this, "Hello world!");
            logger.Info(this, "Hello world!");
        }


        public Task ExecuteAsync(ProtectionContext context)
        {
            foreach (var typeDef in context.ModuleDefMD.Types)
            {
                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (methodDef.HasReturnType)
                        {
                            if (methodDef.IsConstructor == false && methodDef.IsVirtual == false
                                && m_MethodDefCriticalAnalyzer.Analyze(methodDef))
                            {
                                if (methodDef.IsSetter == false && methodDef.IsGetter == false)
                                {
                                    if (methodDef.Parameters.Any(p => p.ParamDef.IsOut == false && p.ParamDef.IsIn == false))
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