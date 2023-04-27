namespace BitMono.Obfuscation.Engine;

public class EngineContextFactory
{
    private readonly ModuleFactoryResult _moduleFactoryResult;
    private readonly ModuleDefinition _runtimeModule;
    private readonly BitMonoContext _context;
    private readonly CancellationToken _cancellationToken;

    public EngineContextFactory(ModuleFactoryResult moduleFactoryResult, ModuleDefinition runtimeModule,
        BitMonoContext context, CancellationToken cancellationToken)
    {
        _moduleFactoryResult = moduleFactoryResult;
        _runtimeModule = runtimeModule;
        _context = context;
        _cancellationToken = cancellationToken;
    }

    public EngineContext Create()
    {
        return new EngineContext
        {
            Module = _moduleFactoryResult.Module,
            RuntimeModule = _runtimeModule,
            ModuleReaderParameters = _moduleFactoryResult.ModuleReaderParameters,
            PEImageBuilder = _moduleFactoryResult.PEImageBuilder,
            RuntimeImporter = _runtimeModule.DefaultImporter,
            BitMonoContext = _context,
            CancellationToken = _cancellationToken,
        };
    }
}