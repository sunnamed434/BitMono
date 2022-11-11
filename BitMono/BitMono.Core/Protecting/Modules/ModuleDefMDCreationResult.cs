using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace BitMono.Core.Protecting.Modules
{
    public class ModuleDefMDCreationResult
    {
        public AssemblyResolver AssemblyResolver { get; set; }
        public ModuleContext ModuleContext { get; set; }
        public ModuleCreationOptions ModuleCreationOptions { get; set; }
        public ModuleDefMD ModuleDefMD { get; set; }
        public ModuleWriterOptions ModuleWriterOptions { get; set; }
    }
}