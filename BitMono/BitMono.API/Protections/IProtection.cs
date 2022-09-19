using System.Threading.Tasks;

namespace BitMono.API.Protections
{
    public interface IProtection
    {
        Task ExecuteAsync(ProtectionContext context);
    }
}