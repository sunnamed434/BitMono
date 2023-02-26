namespace BitMono.Obfuscation.Abstractions;

public class ObfuscationAttributesStrip
{
    public List<CustomAttribute>? ObfuscationAttributesSuccessStrip { get; set; }
    public List<CustomAttribute>? ObfuscationAttributesFailStrip { get; set; }
    public List<CustomAttribute>? ObfuscateAssemblyAttributesSuccessStrip { get; set; }
    public List<CustomAttribute>? ObfuscateAssemblyAttributesFailStrip { get; set; }
}