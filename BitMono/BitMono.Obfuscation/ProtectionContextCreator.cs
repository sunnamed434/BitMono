using BitMono.API.Protecting.Contexts;
using dnlib.DotNet;

namespace BitMono.Obfuscation
{
    public class ProtectionContextCreator
    {
        private readonly ModuleDefMDCreationResult m_ModuleDefMDCreationResult;
        private readonly ModuleDefMD m_ExternalComponentsModuleDefMD;
        private readonly BitMonoContext m_Context;

        public ProtectionContextCreator(ModuleDefMDCreationResult moduleDefMDCreationResult, ModuleDefMD externalComponentsModuleDefMD, BitMonoContext context)
        {
            m_ModuleDefMDCreationResult = moduleDefMDCreationResult;
            m_ExternalComponentsModuleDefMD = externalComponentsModuleDefMD;
            m_Context = context;
        }

        public ProtectionContext Create()
        {
            return new ProtectionContext
            {
                ModuleDefMD = m_ModuleDefMDCreationResult.ModuleDefMD,
                ModuleCreationOptions = m_ModuleDefMDCreationResult.ModuleCreationOptions,
                ModuleWriterOptions = m_ModuleDefMDCreationResult.ModuleWriterOptions,
                ExternalComponentsModuleDefMD = m_ExternalComponentsModuleDefMD,
                Importer = new Importer(m_ModuleDefMDCreationResult.ModuleDefMD),
                ExternalComponentsImporter = new Importer(m_ExternalComponentsModuleDefMD, ImporterOptions.TryToUseMethodDefs),
                BitMonoContext = m_Context,
            };
        }
    }
}