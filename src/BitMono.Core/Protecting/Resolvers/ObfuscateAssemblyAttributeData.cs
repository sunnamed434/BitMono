namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscateAssemblyAttributeData
{
    public bool AssemblyIsPrivate { get; set; }
    public bool StripAfterObfuscation { get; set; }
    public CustomAttribute CustomAttribute { get; set; }
}