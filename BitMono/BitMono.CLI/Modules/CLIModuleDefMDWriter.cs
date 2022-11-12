using BitMono.API.Protecting.Writers;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System.IO;
using System.Threading.Tasks;

namespace BitMono.CLI.Modules
{
    internal class CLIModuleDefMDWriter : IModuleDefMDWriter
    {
        public Task WriteAsync(string outputFile, ModuleDefMD moduleDefMD, ModuleWriterOptions moduleWriterOptions)
        {
            using (moduleDefMD)
            using (var fileStream = File.Create(outputFile))
            {
                moduleDefMD.Write(fileStream, moduleWriterOptions);
            }
            return Task.CompletedTask;
        }
    }
}