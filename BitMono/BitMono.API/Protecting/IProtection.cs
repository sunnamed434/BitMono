using BitMono.API.Protecting.Context;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.API.Protecting
{
    public interface IProtection
    {
        Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default);
    }
}