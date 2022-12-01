using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Injection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class AntiILdasm : IProtection
    {
        private readonly IInjector m_Injector;

        public AntiILdasm(IInjector injector)
        {
            m_Injector = injector;
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            m_Injector.InjectAttribute(context.ModuleDefMD, typeof(SuppressIldasmAttribute).Namespace, nameof(SuppressIldasmAttribute));
            return Task.CompletedTask;
        }
    }
}