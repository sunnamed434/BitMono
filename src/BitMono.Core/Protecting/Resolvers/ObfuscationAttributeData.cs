namespace BitMono.Core.Protecting.Resolvers;

public class ObfuscationAttributeData
{
    public bool ApplyToMembers { get; set; }
    public bool Exclude { get; set; }
    public bool StripAfterObfuscation { get; set; }
    public string Feature { get; set; }
    public CustomAttribute? CustomAttribute { get; set; }
}