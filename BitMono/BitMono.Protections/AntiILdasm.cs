using BitMono.API.Injection;
using BitMono.API.Protections;
using BitMono.Core.Attributes;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    [ExceptRegisterProtection]
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