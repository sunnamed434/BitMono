using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;

namespace BitMono.Obfuscation
{
    public class ModuleDefMDWriterOptionsCreator
    {
        public ModuleWriterOptions Create(ModuleDefMD moduleDefMD)
        {
            var moduleWriterOptions = new ModuleWriterOptions(moduleDefMD);
            moduleWriterOptions.MetadataLogger = DummyLogger.NoThrowInstance;
            moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack | MetadataFlags.PreserveAll;
            moduleWriterOptions.Cor20HeaderOptions.Flags = ComImageFlags.ILOnly;
            return moduleWriterOptions;
        }
    }
}