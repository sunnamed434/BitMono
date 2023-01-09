namespace BitMono.Core.Protecting.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DoNotResolveAttribute : Attribute
{
    public DoNotResolveAttribute(Members members)
    {
        Members = members;
    }

    public Members Members { get; }
}