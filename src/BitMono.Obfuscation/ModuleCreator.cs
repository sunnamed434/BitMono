public class ModuleCreator : IModuleCreator
{
    private readonly byte[] m_Bytes;

    public ModuleCreator(byte[] bytes)
    {
        m_Bytes = bytes;
    }

    public ModuleCreationResult Create()
    {
        var moduleReaderParameters = new ModuleReaderParameters(EmptyErrorListener.Instance);
        var module = SerializedModuleDefinition.FromBytes(m_Bytes, moduleReaderParameters);
        var managedPEImageBuilder = new ManagedPEImageBuilder(MetadataBuilderFlags.PreserveAll);

        var isx86 = module.MachineType == MachineType.I386; 
        if (isx86)
        {
            module.PEKind = OptionalHeaderMagic.PE32;
            module.MachineType = MachineType.I386;
            module.IsBit32Required = true;
        }
        else
        {
            module.PEKind = OptionalHeaderMagic.PE32Plus;
            module.MachineType = MachineType.Amd64;
            module.IsBit32Required = false;
        }

        return new ModuleCreationResult
        {
            Module = module,
            ModuleReaderParameters = moduleReaderParameters,
            PEImageBuilder = managedPEImageBuilder,
        };
    }
}