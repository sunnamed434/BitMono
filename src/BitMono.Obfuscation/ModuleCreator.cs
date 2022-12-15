namespace BitMono.Obfuscation;

public class ModuleCreator : IModuleCreator
{
    private readonly byte[] m_ModuleBytes;

    public ModuleCreator(byte[] moduleBytes)
    {
        m_ModuleBytes = moduleBytes;
    }

    public ModuleCreationResult Create()
    {
        var moduleContext = ModuleDefMD.CreateModuleContext();
        var moduleCreationOptions = new ModuleCreationOptions(moduleContext, CLRRuntimeReaderKind.Mono);
        var moduleDefMD = ModuleDefMD.Load(m_ModuleBytes, moduleCreationOptions);
        var moduleDefMDWriterOptions = new ModuleWriterOptionsCreator().Create(moduleDefMD);
        return new ModuleCreationResult
        {
            ModuleCreationOptions = moduleCreationOptions,
            ModuleDefMD = moduleDefMD,
            ModuleWriterOptions = moduleDefMDWriterOptions,
        };
    }
}