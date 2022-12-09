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
            var moduleContext = new ModuleContextCreator().Create();
            var moduleCreationOptions = new ModuleCreationOptionsCreator().Create(moduleContext);
            var moduleDefMD = ModuleDefMD.Load(m_ModuleBytes, moduleCreationOptions);
            var moduleDefMDWriterOptions = new ModuleDefMDWriterOptionsCreator().Create(moduleDefMD);
            return new ModuleDefMDCreationResult
            {
                ModuleContext = moduleContext,
                AssemblyResolver = moduleContext.AssemblyResolver,
                ModuleCreationOptions = moduleCreationOptions,
                ModuleDefMD = moduleDefMD,
                ModuleWriterOptions = moduleDefMDWriterOptions,
            };
        }
    }
}