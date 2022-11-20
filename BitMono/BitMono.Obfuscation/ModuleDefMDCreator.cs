using BitMono.Obfuscation.API;
using dnlib.DotNet;

namespace BitMono.Obfuscation
{
    public class ModuleDefMDCreator : IModuleDefMDCreator
    {
        private readonly byte[] m_ModuleBytes;

        public ModuleDefMDCreator(byte[] moduleBytes)
        {
            m_ModuleBytes = moduleBytes;
        }

        public ModuleDefMDCreationResult Create()
        {
            var moduleContext = ModuleDefMD.CreateModuleContext();
            var moduleCreationOptions = new ModuleCreationOptions(moduleContext, CLRRuntimeReaderKind.Mono);
            var moduleDefMD = ModuleDefMD.Load(m_ModuleBytes, moduleCreationOptions);

            var moduleDefMDWriterOptions = new ModuleDefMDWriterOptionsCreator().Create(moduleDefMD);
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