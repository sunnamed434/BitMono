using System.Threading.Tasks;

namespace BitMono.API.Protecting
{
    public interface IProtection
    {
        Task ExecuteAsync(ProtectionContext context);
    }
}