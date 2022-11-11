using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using System.Threading.Tasks;

namespace BitMono.Obfuscation
{
    public class ModuleDefMDWriterOptionsCreator
    {
        public Task<ModuleWriterOptions> CreateAsync(ModuleDefMD moduleDefMD)
        {
            var moduleDefMDWriterOptions = new ModuleWriterOptions(moduleDefMD);
            moduleDefMDWriterOptions.MetadataLogger = DummyLogger.NoThrowInstance;
            moduleDefMDWriterOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack | MetadataFlags.PreserveAll;
            moduleDefMDWriterOptions.Cor20HeaderOptions.Flags = ComImageFlags.ILOnly;
            return Task.FromResult(moduleDefMDWriterOptions);
        }
    }
}