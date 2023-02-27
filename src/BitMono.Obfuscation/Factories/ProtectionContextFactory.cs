namespace BitMono.Obfuscation.Factories;

public class ProtectionContextFactory
{
    private readonly ModuleFactoryResult _moduleFactoryResult;
    private readonly ModuleDefinition _runtimeModule;
    private readonly BitMonoContext _context;
    private readonly CancellationToken _cancellationToken;

    public ProtectionContextFactory(ModuleFactoryResult moduleFactoryResult, ModuleDefinition runtimeModule, BitMonoContext context, CancellationToken cancellationToken)
    {
        _moduleFactoryResult = moduleFactoryResult;
        _runtimeModule = runtimeModule;
        _context = context;
        _cancellationToken = cancellationToken;
    }

    public ProtectionContext Create()
    {
        return new ProtectionContext
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