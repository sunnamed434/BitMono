namespace BitMono.Core.Resolvers;

public class ObfuscateAssemblyAttributeData
{
    public ObfuscateAssemblyAttributeData()
    {
    }
    public ObfuscateAssemblyAttributeData(bool assemblyIsPrivate, bool stripAfterObfuscation,
        CustomAttribute? customAttribute)
    {
        AssemblyIsPrivate = assemblyIsPrivate;
        StripAfterObfuscation = stripAfterObfuscation;
        CustomAttribute = customAttribute;
    }

    public bool AssemblyIsPrivate { get; set; }
    public bool StripAfterObfuscation { get; set; }
    public CustomAttribute? CustomAttribute { get; set; }
}