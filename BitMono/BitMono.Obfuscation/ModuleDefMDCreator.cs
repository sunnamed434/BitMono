using dnlib.DotNet;
using System.Threading.Tasks;

namespace BitMono.Obfuscation
{
    public class ModuleDefMDCreator
    {
        public async Task<ModuleDefMDCreationResult> CreateAsync(string moduleFile)
        {
            var assemblyResolver = new AssemblyResolver();
            var moduleContext = new ModuleContext(assemblyResolver);
            assemblyResolver.DefaultModuleContext = moduleContext;
            var moduleCreationOptions = new ModuleCreationOptions(assemblyResolver.DefaultModuleContext, CLRRuntimeReaderKind.Mono);
            var moduleDefMD = ModuleDefMD.Load(moduleFile, moduleCreationOptions);

            var moduleDefMDWriterOptions = await new ModuleDefMDWriterOptionsCreator().CreateAsync(moduleDefMD);
            return new ModuleDefMDCreationResult
            {
                AssemblyResolver = assemblyResolver,
                ModuleContext = moduleContext,
                ModuleCreationOptions = moduleCreationOptions,
                ModuleDefMD = moduleDefMD,
                ModuleWriterOptions = moduleDefMDWriterOptions,
            };
        }
    }
}