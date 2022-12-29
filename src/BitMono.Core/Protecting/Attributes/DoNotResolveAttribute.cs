namespace BitMono.Core.Protecting.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DoNotResolveAttribute : Attribute
{
    public DoNotResolveAttribute(Members resolves)
    {
        Resolves = resolves;
    }

    public Members Resolves { get; }
}