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

        ConfigureForNativeCode(module);

        var managedPEImageBuilder =
            new ManagedPEImageBuilder(new DotNetDirectoryFactory(_metadataBuilderFlags), _errorListener);

        return new ModuleFactoryResult
        {
            Module = module,
            ModuleReaderParameters = moduleReaderParameters,
            PEImageBuilder = managedPEImageBuilder,
        };
    }

    /// <summary>
    /// This is necessary to make native code work inside the assembly.
    /// See more here: https://docs.washi.dev/asmresolver/guides/dotnet/unmanaged-method-bodies.html
    /// However, sometimes it causes issues with the assembly like `System.BadImageFormatException`
    /// at the end when running the protected file, so that's why it's here but not at some startup point.
    /// </summary>
    private void ConfigureForNativeCode(ModuleDefinition module)
    {
        if (_obfuscationSettings.ConfigureForNativeCode == false)
        {
            return;
        }

        module.IsILOnly = false;
        var x64 = module.MachineType == MachineType.Amd64;
        if (x64)
        {
            module.PEKind = OptionalHeaderMagic.PE32Plus;
            module.MachineType = MachineType.Amd64;
            module.IsBit32Required = false;
        }
        else
        {
            module.PEKind = OptionalHeaderMagic.PE32;
            module.MachineType = MachineType.I386;
            module.IsBit32Required = true;
            module.IsBit32Preferred = false;
        }
    }
}