using BitMono.API.Protecting;
using BitMono.Core.Protecting.Analyzing;
using BitMono.Utilities.Extensions.Dnlib;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class ObjectReturnType : IProtection
    {
        private readonly MethodDefCriticalAnalyzer m_MethodDefCriticalAnalyzer;

        public ObjectReturnType(MethodDefCriticalAnalyzer methodDefCriticalAnalyzer)
        {
            m_MethodDefCriticalAnalyzer = methodDefCriticalAnalyzer;
        }


        public Task ExecuteAsync(ProtectionContext context)
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
                                && m_MethodDefCriticalAnalyzer.NotCriticalToMakeChanges(context, methodDef)
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