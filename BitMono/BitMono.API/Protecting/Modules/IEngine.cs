using System.Threading;
using System.Threading.Tasks;

namespace BitMono.API.Protecting.Modules
{
    public interface IEngine
    {
        Task StartAsync(CancellationToken cancellationToken = default);
    }
}