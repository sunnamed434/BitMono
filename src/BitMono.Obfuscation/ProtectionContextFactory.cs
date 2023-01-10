namespace BitMono.Obfuscation;

public class ProtectionContextFactory
{
    private readonly ModuleFactoryResult m_ModuleFactoryResult;
    private readonly ModuleDefinition m_RuntimeModule;
    private readonly BitMonoContext m_Context;
    private readonly CancellationToken m_CancellationToken;

    public ProtectionContextFactory(ModuleFactoryResult moduleFactoryResult, ModuleDefinition runtimeModule, BitMonoContext context, CancellationToken cancellationToken)
    {
        m_ModuleFactoryResult = moduleFactoryResult;
        m_RuntimeModule = runtimeModule;
        m_Context = context;
        m_CancellationToken = cancellationToken;
    }

    public ProtectionContext Create()
    {
        return new ProtectionContext
        {
            Module = m_ModuleFactoryResult.Module,
            RuntimeModule = m_RuntimeModule,
            ModuleReaderParameters = m_ModuleFactoryResult.ModuleReaderParameters,
            PEImageBuilder = m_ModuleFactoryResult.PEImageBuilder,
            RuntimeImporter = new ReferenceImporter(m_RuntimeModule),
            BitMonoContext = m_Context,
            CancellationToken = m_CancellationToken,
        };
    }
}