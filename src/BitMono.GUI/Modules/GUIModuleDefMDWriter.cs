using BitMono.API.Protecting.Writers;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace BitMono.GUI.Modules
{
    internal class GUIModuleDefMDWriter : IModuleDefMDWriter
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