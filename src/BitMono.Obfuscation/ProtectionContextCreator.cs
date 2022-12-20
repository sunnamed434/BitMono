namespace BitMono.Obfuscation;

public class ProtectionContextCreator
{
    private readonly ModuleCreationResult m_ModuleCreationResult;
    private readonly ModuleDefinition m_RuntimeModuleDefinition;
    private readonly BitMonoContext m_Context;

    public ProtectionContextCreator(
        ModuleCreationResult moduleCreationResult, 
        ModuleDefinition runtimeModuleDefinition, 
        BitMonoContext context)
    {
        m_ModuleCreationResult = moduleCreationResult;
        m_RuntimeModuleDefinition = runtimeModuleDefinition;
        m_Context = context;
    }

    public ProtectionContext Create()
    {
        return new ProtectionContext
        {
            Module = m_ModuleCreationResult.Module,
            RuntimeModule = m_RuntimeModuleDefinition,
            ModuleReaderParameters = m_ModuleCreationResult.ModuleReaderParameters,
            PEImageBuilder = m_ModuleCreationResult.PEImageBuilder,
            RuntimeImporter = new ReferenceImporter(m_RuntimeModuleDefinition),
            BitMonoContext = m_Context,
        };
    }
}