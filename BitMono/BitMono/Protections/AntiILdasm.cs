using BitMono.API.Injection;
using BitMono.API.Protections;
using BitMono.Core.Protections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BitMono.Packers
{
    public class AntiILdasm : IProtection
    {
        private readonly ProtectionContext m_Context;
        private readonly IInjector m_Injector;

        public AntiILdasm(ProtectionContext context, IInjector injector)
        {
            m_Context = context;
            m_Injector = injector;
        }


        public Task ExecuteAsync()
        {
            m_Injector.InjectAttribute(m_Context.ModuleDefMD, typeof(SuppressIldasmAttribute).Namespace, nameof(SuppressIldasmAttribute));
            return Task.CompletedTask;
        }
    }
}