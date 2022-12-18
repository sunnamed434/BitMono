using AsmResolver.DotNet;
using AsmResolver.DotNet.Serialized;

namespace BitMono.Obfuscation;

public class ModuleCreator : IModuleCreator
{
    private readonly byte[] m_Bytes;

    public ModuleCreator(byte[] bytes)
    {
        m_Bytes = bytes;
    }

    public ModuleCreationResult Create()
    {
        var moduleContext = ModuleDefMD.CreateModuleContext();
        var moduleCreationOptions = new ModuleCreationOptions(moduleContext, CLRRuntimeReaderKind.Mono);
        var moduleDefention = ModuleDefinition.FromBytes(m_Bytes, new ModuleReaderParameters());
        var moduleDefMD = ModuleDefMD.Load(m_Bytes, moduleCreationOptions);
        var ModuleWriterOptions = new ModuleWriterOptionsCreator().Create(moduleDefMD);
        return new ModuleCreationResult
        {
            ModuleCreationOptions = moduleCreationOptions,
            ModuleDefMD = moduleDefMD,
            ModuleWriterOptions = ModuleWriterOptions,
        };
    }
}