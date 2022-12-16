namespace BitMono.Obfuscation;

public class ProtectionContextCreator
{
    private readonly ModuleCreationResult m_ModuleCreationResult;
    private readonly ModuleDefMD m_RuntimeModuleDefMD;
    private readonly BitMonoContext m_Context;

    public ProtectionContextCreator(
        ModuleCreationResult moduleCreationResult, 
        ModuleDefMD runtimeModuleDefMD, 
        BitMonoContext context)
    {
        m_ModuleCreationResult = moduleCreationResult;
        m_RuntimeModuleDefMD = runtimeModuleDefMD;
        m_Context = context;
    }

    public ProtectionContext Create()
    {
        return new ProtectionContext
        {
            ModuleDefMD = m_ModuleCreationResult.ModuleDefMD,
            ModuleCreationOptions = m_ModuleCreationResult.ModuleCreationOptions,
            ModuleWriterOptions = m_ModuleCreationResult.ModuleWriterOptions,
            RuntimeModuleDefMD = m_RuntimeModuleDefMD,
            Importer = new Importer(m_ModuleCreationResult.ModuleDefMD),
            RuntimeImporter = new Importer(m_RuntimeModuleDefMD, ImporterOptions.TryToUseDefs),
            BitMonoContext = m_Context,
        };
    }
}