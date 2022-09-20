using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System.Reflection;

namespace BitMono.API.Protections
{
    public class ProtectionContext
    {
        public ProtectionContext(
            ModuleDefMD moduleDefMD, 
            ModuleWriterOptions moduleWriterOptions, 
            ModuleDefMD encryptionModuleDefMD, 
            Assembly targetAssembly)
        {
            ModuleDefMD = moduleDefMD;
            ModuleWriterOptions = moduleWriterOptions;
            EncryptionModuleDefMD = encryptionModuleDefMD;
            TargetAssembly = targetAssembly;
        }


        public ModuleDefMD ModuleDefMD { get; set; }
        public ModuleWriterOptions ModuleWriterOptions { get; set; }
        public ModuleDefMD EncryptionModuleDefMD { get; set; }
        public Assembly TargetAssembly { get; set; }
    }
}