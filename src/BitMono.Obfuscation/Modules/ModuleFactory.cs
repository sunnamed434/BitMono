namespace BitMono.Obfuscation.Modules;

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
        var managedPEImageBuilder = new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll);

        return new ModuleFactoryResult
        {
            Module = module,
            ModuleReaderParameters = moduleReaderParameters,
            PEImageBuilder = managedPEImageBuilder,
        };
    }
}