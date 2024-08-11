namespace BitMono.Obfuscation.Modules;

public class ModuleFactory : IModuleFactory
{
    private readonly byte[] _bytes;
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly IErrorListener _errorListener;
    private readonly MetadataBuilderFlags _metadataBuilderFlags;

    public ModuleFactory(byte[] bytes, ObfuscationSettings obfuscationSettings, IErrorListener errorListener,
        MetadataBuilderFlags metadataBuilderFlags = MetadataBuilderFlags.None)
    {
        _bytes = bytes;
        _obfuscationSettings = obfuscationSettings;
        _errorListener = errorListener;
        _metadataBuilderFlags = metadataBuilderFlags;
    }

    public ModuleFactoryResult Create()
    {
        var moduleReaderParameters = new ModuleReaderParameters(_errorListener)
        {
            PEReaderParameters = new PEReaderParameters(_errorListener)
        };
        var module = ModuleDefinition.FromBytes(_bytes, moduleReaderParameters);

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