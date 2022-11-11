using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System.Threading.Tasks;

namespace BitMono.API.Protecting.Writers
{
    public interface IModuleDefMDWriter
    {
        Task WriteAsync(string outputFile, ModuleDefMD moduleDefMD, ModuleWriterOptions moduleWriterOptions);
    }
}