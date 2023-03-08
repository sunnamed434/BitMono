namespace BitMono.Obfuscation.Factories;

public class ModuleFactory : IModuleFactory
{
    private readonly byte[] _bytes;
    private readonly IErrorListener _errorListener;

    public ModuleFactory(byte[] bytes, IErrorListener errorListener)
    {
        _bytes = bytes;
        _errorListener = errorListener;
    }

    public ModuleFactoryResult Create()
    {
        var moduleReaderParameters = new ModuleReaderParameters(_errorListener);
        var module = SerializedModuleDefinition.FromBytes(_bytes, moduleReaderParameters);
        var managedPEImageBuilder = new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll);

        return new ModuleFactoryResult
        {
            Module = module,
            ModuleReaderParameters = moduleReaderParameters,
            PEImageBuilder = managedPEImageBuilder,
        };
    }
}