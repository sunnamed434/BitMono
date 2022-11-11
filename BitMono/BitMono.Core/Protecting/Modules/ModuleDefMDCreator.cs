using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using System.Threading.Tasks;

namespace BitMono.Core.Protecting.Modules
{
    public class ModuleDefMDCreator
    {
        public Task<ModuleDefMDCreationResult> CreateAsync(string moduleFile)
        {
            var assemblyResolver = new AssemblyResolver();
            var moduleContext = new ModuleContext(assemblyResolver);
            assemblyResolver.DefaultModuleContext = moduleContext;
            var moduleCreationOptions = new ModuleCreationOptions(assemblyResolver.DefaultModuleContext, CLRRuntimeReaderKind.Mono);
            var moduleDefMD = ModuleDefMD.Load(moduleFile, moduleCreationOptions);
            var moduleWriterOptions = new ModuleWriterOptions(moduleDefMD);
            moduleWriterOptions.MetadataLogger = DummyLogger.NoThrowInstance;
            moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack | MetadataFlags.PreserveAll;
            moduleWriterOptions.Cor20HeaderOptions.Flags = ComImageFlags.ILOnly;
            return Task.FromResult(new ModuleDefMDCreationResult
            {
                AssemblyResolver = assemblyResolver,
                ModuleContext = moduleContext,
                ModuleCreationOptions = moduleCreationOptions,
                ModuleDefMD = moduleDefMD,
                ModuleWriterOptions = moduleWriterOptions,
            });
        }
    }
}