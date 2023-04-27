namespace BitMono.Obfuscation.Modules;

public class ModuleFactory : IModuleFactory
{
    private readonly byte[] _bytes;
    private readonly IErrorListener _errorListener;
    private readonly MetadataBuilderFlags _metadataBuilderFlags;

    public ModuleFactory(byte[] bytes, IErrorListener errorListener,
        MetadataBuilderFlags metadataBuilderFlags = MetadataBuilderFlags.None)
    {
        _bytes = bytes;
        _errorListener = errorListener;
        _metadataBuilderFlags = metadataBuilderFlags;
    }

    public ModuleFactoryResult Create()
    {
        var moduleReaderParameters = new ModuleReaderParameters(_errorListener);
        var module = ModuleDefinition.FromBytes(_bytes, moduleReaderParameters);
        module.Attributes &= ~DotNetDirectoryFlags.ILOnly;
        var x86 = module.MachineType == MachineType.I386;
        if (x86)
        {
            module.PEKind = OptionalHeaderMagic.PE32;
            module.MachineType = MachineType.I386;
            module.Attributes |= DotNetDirectoryFlags.Bit32Required;
        }
        else
        {
            module.PEKind = OptionalHeaderMagic.PE32Plus;
            module.MachineType = MachineType.Amd64;
        }
        var managedPEImageBuilder = new ManagedPEImageBuilder(_metadataBuilderFlags);

        return new ModuleFactoryResult
        {
            Module = module,
            ModuleReaderParameters = moduleReaderParameters,
            PEImageBuilder = managedPEImageBuilder,
        };
    }
}