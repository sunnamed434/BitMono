namespace BitMono.Obfuscation.API;

public class ModuleCreationResult
{
    public ModuleDefinition Module { get; set; }
    public ModuleReaderParameters ModuleReaderParameters { get; set; }
    public IPEImageBuilder PEImageBuilder { get; set; }
}