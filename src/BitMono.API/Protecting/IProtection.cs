using BitMono.API.Protecting.Contexts;
using BitMono.Core.Protecting;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.API.Protecting
{
    public interface IProtection
    {
        Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default);
    }
}