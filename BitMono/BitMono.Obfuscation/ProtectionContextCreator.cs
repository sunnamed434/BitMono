using BitMono.API.Protecting.Context;
using dnlib.DotNet;
using System.Threading.Tasks;

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

        public Task<ProtectionContext> CreateAsync()
        {
            return Task.FromResult(new ProtectionContext
            {
                ModuleDefMD = m_ModuleDefMDCreationResult.ModuleDefMD,
                ModuleCreationOptions = m_ModuleDefMDCreationResult.ModuleCreationOptions,
                ModuleWriterOptions = m_ModuleDefMDCreationResult.ModuleWriterOptions,
                ExternalComponentsModuleDefMD = m_ExternalComponentsModuleDefMD,
                Importer = new Importer(m_ModuleDefMDCreationResult.ModuleDefMD),
                ExternalComponentsImporter = new Importer(m_ExternalComponentsModuleDefMD, ImporterOptions.TryToUseMethodDefs),
                BitMonoContext = m_Context,
            });
        }
    }
}