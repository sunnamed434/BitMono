using dnlib.DotNet;
using System.Threading.Tasks;

namespace BitMono.API.Protecting.Writers
{
    public interface IModuleDefMDWriter
    {
        Task WriteAsync(ModuleDefMD moduleDefMD);
    }
}