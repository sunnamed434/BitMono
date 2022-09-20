using BitMono.API.Protections;
using BitMono.Core.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    [ExceptRegisterProtection]
    public class CallToCalli : IProtection
    {
        public Task ExecuteAsync(ProtectionContext context)
        {
            foreach (var type in context.ModuleDefMD.Types.ToArray())
            {
                foreach (var method in type.Methods.ToArray())
                {
                    if (method.IsVirtual == false)
                    {
                        if (method.HasBody)
                        {
                            if (method.Body.HasInstructions)
                            {
                                
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
