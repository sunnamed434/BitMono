namespace BitMono.Obfuscation.Stripping;

public class ObfuscationAttributesStrip
{
#pragma warning disable CS8618
    public List<CustomAttribute> ObfuscationAttributesSuccessStrip { get; set; }
    public List<CustomAttribute> ObfuscationAttributesFailStrip { get; set; }
    public List<CustomAttribute> ObfuscateAssemblyAttributesSuccessStrip { get; set; }
    public List<CustomAttribute> ObfuscateAssemblyAttributesFailStrip { get; set; }
#pragma warning restore CS8618
}
