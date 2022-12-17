namespace BitMono.Core.Protecting.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ProtectionNameAttribute : Attribute
{
    public ProtectionNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}