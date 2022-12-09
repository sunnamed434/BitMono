using BitMono.API.Protecting.Writers;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System.Threading.Tasks;

namespace BitMono.CLI.Modules
{
    internal class CLIModuleDefMDWriter : IModuleDefMDWriter
    {
        public Task WriteAsync(string outputFile, ModuleDefMD moduleDefMD, ModuleWriterOptions moduleWriterOptions)
        {
            using (moduleDefMD)
            {
                moduleDefMD.Write(outputFile, moduleWriterOptions);
            }
            return Task.CompletedTask;
        }
    }
}