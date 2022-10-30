using System.Threading;
using System.Threading.Tasks;
using BitMono.API.Protecting.Contexts;

namespace BitMono.API.Protecting
{
    public interface IProtection
    {
        Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default);
    }
}