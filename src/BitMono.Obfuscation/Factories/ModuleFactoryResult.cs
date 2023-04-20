namespace BitMono.Obfuscation.Factories;

public class ModuleFactoryResult
{
#pragma warning disable CS8618
    public ModuleDefinition Module { get; set; }
    public ModuleReaderParameters ModuleReaderParameters { get; set; }
    public IPEImageBuilder PEImageBuilder { get; set; }
#pragma warning restore CS8618
}