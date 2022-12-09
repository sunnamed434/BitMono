using BitMono.API.Protecting.Contexts;
using dnlib.DotNet;

namespace BitMono.Obfuscation
{
    public class ProtectionContextCreator
    {
        private readonly ModuleDefMDCreationResult m_ModuleDefMDCreationResult;
        private readonly ModuleDefMD m_RuntimeModuleDefMD;
        private readonly BitMonoContext m_Context;

        public ProtectionContextCreator(
            ModuleDefMDCreationResult moduleDefMDCreationResult, 
            ModuleDefMD externalComponentsModuleDefMD, 
            BitMonoContext context)
        {
            m_ModuleDefMDCreationResult = moduleDefMDCreationResult;
            m_RuntimeModuleDefMD = externalComponentsModuleDefMD;
            m_Context = context;
        }

        public ProtectionContext Create()
        {
            return new ProtectionContext
            {
                ModuleDefMD = m_ModuleDefMDCreationResult.ModuleDefMD,
                ModuleCreationOptions = m_ModuleDefMDCreationResult.ModuleCreationOptions,
                ModuleWriterOptions = m_ModuleDefMDCreationResult.ModuleWriterOptions,
                RuntimeModuleDefMD = m_RuntimeModuleDefMD,
                Importer = new Importer(m_ModuleDefMDCreationResult.ModuleDefMD),
                RuntimeImporter = new Importer(m_RuntimeModuleDefMD, ImporterOptions.TryToUseDefs),
                BitMonoContext = m_Context,
            };
        }
    }
}