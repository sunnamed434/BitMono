using BitMono.Obfuscation.API;
using dnlib.DotNet;
using System.Threading.Tasks;

namespace BitMono.Obfuscation
{
    public class ModuleDefMDCreator : IModuleDefMDCreator
    {
        private readonly byte[] m_ModuleBytes;

        public ModuleDefMDCreator(byte[] moduleBytes)
        {
            m_ModuleBytes = moduleBytes;
        }

        public async Task<ModuleDefMDCreationResult> CreateAsync()
        {
            var moduleContext = ModuleDefMD.CreateModuleContext();
            var moduleCreationOptions = new ModuleCreationOptions(moduleContext, CLRRuntimeReaderKind.Mono);
            var moduleDefMD = ModuleDefMD.Load(m_ModuleBytes, moduleCreationOptions);

            var moduleDefMDWriterOptions = await new ModuleDefMDWriterOptionsCreator().CreateAsync(moduleDefMD);
            return new ModuleDefMDCreationResult
            {
                AssemblyResolver = moduleContext.AssemblyResolver,
                ModuleContext = moduleContext,
                ModuleCreationOptions = moduleCreationOptions,
                ModuleDefMD = moduleDefMD,
                ModuleWriterOptions = moduleDefMDWriterOptions,
            };
        }
    }
}