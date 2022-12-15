namespace BitMono.Obfuscation.API;

public class ModuleCreationResult
{
    public ModuleDefMD ModuleDefMD { get; set; }
    public ModuleCreationOptions ModuleCreationOptions { get; set; }
    public ModuleWriterOptions ModuleWriterOptions { get; set; }
}