using BitMono.API.Protecting;
using BitMono.API.Protecting.Renaming;
using BitMono.Core.Protecting.Analyzing;
using BitMono.Utilities.Extensions.Dnlib;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class FullRenamer : IProtection
    {
        private readonly TypeDefCriticalAnalyzer m_TypeDefCriticalAnalyzer;
        private readonly MethodDefCriticalAnalyzer m_MethodDefCriticalAnalyzer;
        private readonly IRenamer m_Renamer;

        public FullRenamer(TypeDefCriticalAnalyzer typeDefCriticalAnalyzer, MethodDefCriticalAnalyzer methodDefCriticalAnalyzer, IRenamer renamer)
        {
            m_TypeDefCriticalAnalyzer = typeDefCriticalAnalyzer;
            m_MethodDefCriticalAnalyzer = methodDefCriticalAnalyzer;
            m_Renamer = renamer;
        }


        public Task ExecuteAsync(ProtectionContext context)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (typeDef.IsGlobalModuleType == false
                    && m_TypeDefCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef))
                {
                    m_Renamer.Rename(context, typeDef);
                    if (typeDef.HasMethods)
                    {
                        foreach (var methodDef in typeDef.Methods.ToArray())
                        {
                            if (methodDef.IsConstructor == false
                                && methodDef.IsVirtual == false
                                && m_MethodDefCriticalAnalyzer.NotCriticalToMakeChanges(context, methodDef))
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