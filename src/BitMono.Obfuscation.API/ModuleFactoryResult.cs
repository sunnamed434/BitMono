namespace BitMono.Obfuscation.API;

public class ModuleFactoryResult
{
    public ModuleDefinition Module { get; set; }
    public ModuleReaderParameters ModuleReaderParameters { get; set; }
    public IPEImageBuilder PEImageBuilder { get; set; }
}