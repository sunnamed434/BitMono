using BitMono.API.Protecting.Context;
using System.Threading.Tasks;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IBitMonoModuleFileResolver
    {
        Task ResolveAsync(BitMonoContext context);
    }
}