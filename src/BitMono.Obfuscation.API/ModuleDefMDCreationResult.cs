using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace BitMono.Obfuscation
{
    public class ModuleDefMDCreationResult
    {
        public ModuleContext ModuleContext { get; set; }
        public IAssemblyResolver AssemblyResolver { get; set; }
        public ModuleCreationOptions ModuleCreationOptions { get; set; }
        public ModuleDefMD ModuleDefMD { get; set; }
        public ModuleWriterOptions ModuleWriterOptions { get; set; }
    }
}