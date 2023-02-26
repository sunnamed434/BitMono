namespace BitMono.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ProtectionDescriptionAttribute : Attribute
{
    public ProtectionDescriptionAttribute(string description)
    {
        Description = description;
    }

    public string Description { get; }
}