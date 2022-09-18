using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System.Reflection;

namespace BitMono.Core.Protections
{
    public class ProtectionContext
    {
        public readonly ModuleDefMD ModuleDefMD;
        public readonly ModuleWriterOptions ModuleWriterOptions;
        public readonly ModuleDefMD EncryptionModuleDefMD;
        public readonly Assembly TargetAssembly;

        public ProtectionContext(ModuleDefMD moduleDefMD, ModuleWriterOptions moduleWriterOptions, ModuleDefMD encryptionModuleDefMD, Assembly targetAssembly)
        {
            ModuleDefMD = moduleDefMD;
            ModuleWriterOptions = moduleWriterOptions;
            EncryptionModuleDefMD = encryptionModuleDefMD;
            TargetAssembly = targetAssembly;
        }
    }
}