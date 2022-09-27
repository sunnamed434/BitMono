using BitMono.API.Protecting;
using BitMono.Core.Protecting.Analyzing;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class NoNamespaces : IProtection
    {
        private readonly TypeDefCriticalAnalyzer m_TypeDefCriticalAnalyzer;
        private readonly NameCriticalAnalyzer m_NameCriticalAnalyzer;

        public NoNamespaces(TypeDefCriticalAnalyzer typeDefCriticalAnalyzer, NameCriticalAnalyzer nameCriticalAnalyzer)
        {
            m_TypeDefCriticalAnalyzer = typeDefCriticalAnalyzer;
            m_NameCriticalAnalyzer = nameCriticalAnalyzer;
        }


        public Task ExecuteAsync(ProtectionContext context)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (typeDef.IsGlobalModuleType == false
                    && m_TypeDefCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef)
                    && typeDef.Namespace?.Data != null)
                {
                    if (m_NameCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef.Namespace))
                    {
                        typeDef.Namespace = string.Empty;
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}