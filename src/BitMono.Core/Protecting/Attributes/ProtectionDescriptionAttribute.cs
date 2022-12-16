namespace BitMono.Core.Protecting.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ProtectionDescriptionAttribute : Attribute
{
    public ProtectionDescriptionAttribute(string description)
    {
        Description = description;
    }

    public string Description { get; }
}