using BitMono.API.Injection;
using BitMono.API.Protections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BitMono.Packers
{
    public class AntiILdasm : IProtection
    {
        private readonly IInjector m_Injector;

        public AntiILdasm(IInjector injector)
        {
            m_Injector = injector;
        }


        public Task ExecuteAsync(ProtectionContext context)
        {
            m_Injector.InjectAttribute(context.ModuleDefMD, typeof(SuppressIldasmAttribute).Namespace, nameof(SuppressIldasmAttribute));
            return Task.CompletedTask;
        }
    }
}