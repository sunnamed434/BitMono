namespace BitMono.Core.Protecting.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public abstract class RuntimeMonikerAttribute : Attribute
{
    protected RuntimeMonikerAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public virtual string GetMessage()
    {
        return $"Intended for {Name} runtime";
    }
}