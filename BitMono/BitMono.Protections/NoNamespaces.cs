using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.Core.Protecting.Analyzing.DnlibDefs;
using BitMono.Core.Protecting.Analyzing.Naming;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class NoNamespaces : IProtection
    {
        private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
        private readonly NameCriticalAnalyzer m_NameCriticalAnalyzer;

        public NoNamespaces(DnlibDefCriticalAnalyzer typeDefCriticalAnalyzer, NameCriticalAnalyzer nameCriticalAnalyzer)
        {
            m_DnlibDefCriticalAnalyzer = typeDefCriticalAnalyzer;
            m_NameCriticalAnalyzer = nameCriticalAnalyzer;
        }


        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            foreach (var typeDef in context.ModuleDefMD.GetTypes().ToArray())
            {
                if (typeDef.IsGlobalModuleType == false
                    && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(context, typeDef)
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