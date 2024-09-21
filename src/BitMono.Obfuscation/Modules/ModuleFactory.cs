namespace BitMono.Obfuscation.Modules;

public class ModuleFactory : IModuleFactory
{
    private readonly byte[] _bytes;
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly IErrorListener _errorListener;
    private readonly ILogger _logger;
    private readonly MetadataBuilderFlags _metadataBuilderFlags;

    public ModuleFactory(
        byte[] bytes, ObfuscationSettings obfuscationSettings,
        IErrorListener errorListener, ILogger logger,
        MetadataBuilderFlags metadataBuilderFlags = MetadataBuilderFlags.None)
    {
        _bytes = bytes;
        _obfuscationSettings = obfuscationSettings;
        _errorListener = errorListener;
        _logger = logger.ForContext<ModuleFactory>();
        _metadataBuilderFlags = metadataBuilderFlags;
    }

    public ModuleFactoryResult Create()
    {
        var moduleReaderParameters = new ModuleReaderParameters(_errorListener)
        {
            PEReaderParameters = new PEReaderParameters(_errorListener)
        };

        ModuleDefinition module;
        try
        {

            module = ModuleDefinition.FromBytes(_bytes, moduleReaderParameters);
        }
        catch (BadImageFormatException)
        {
            _logger.Error(
                "The file appears to be using native code (ReadyToRun). " +
                "Ensure you're using a `.dll` file with non-native (managed) code. " +
                "If the file is using ReadyToRun, you can disable it by adding `<PublishReadyToRun>false</PublishReadyToRun>` to your `.csproj` file. " +
                "For more information, visit: https://bitmono.readthedocs.io/en/latest/obfuscationissues/ready-to-run.html " +
                "Alternatively, the file might be broken, protected, or obfuscated. " +
                "If neither of these cases apply, please contact us for support."
            );
            throw;
        }

        var managedPEImageBuilder =
            new ManagedPEImageBuilder(new DotNetDirectoryFactory(_metadataBuilderFlags), _errorListener);

        return new ModuleFactoryResult
        {
            Module = module,
            ModuleReaderParameters = moduleReaderParameters,
            PEImageBuilder = managedPEImageBuilder,
        };
    }
}