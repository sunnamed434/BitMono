using BitMono.API.Protecting;
using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Injection;
using BitMono.Core.Protecting;
using BitMono.Core.Protecting.Attributes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    [ProtectionName(nameof(AntiILdasm))]
    public class AntiILdasm : IProtection
    {
        private readonly IInjector m_Injector;

        public AntiILdasm(IInjector injector)
        {
            m_Injector = injector;
        }

        public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
        {
            m_Injector.InjectAttribute(context.ModuleDefMD, typeof(SuppressIldasmAttribute).Namespace, nameof(SuppressIldasmAttribute));
            return Task.CompletedTask;
        }
    }
}