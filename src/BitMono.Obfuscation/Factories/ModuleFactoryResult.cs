namespace BitMono.Obfuscation.Factories;

public class ModuleFactoryResult
{
    public ModuleDefinition? Module { get; set; }
    public ModuleReaderParameters? ModuleReaderParameters { get; set; }
    public IPEImageBuilder? PEImageBuilder { get; set; }
}