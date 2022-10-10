using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System.Reflection;

namespace BitMono.API.Protecting
{
    public class ProtectionContext
    {
        public ModuleDefMD ModuleDefMD { get; set; }
        public ModuleCreationOptions ModuleCreationOptions { get; set; }
        public ModuleWriterOptions ModuleWriterOptions { get; set; }
        public ModuleDefMD EncryptionModuleDefMD { get; set; }
        public Assembly Assembly { get; set; }
        public BitMonoContext BitMonoContext { get; set; }

        public AssemblyResolver AssemblyResolver => (AssemblyResolver)ModuleCreationOptions.Context.AssemblyResolver;
    }
}