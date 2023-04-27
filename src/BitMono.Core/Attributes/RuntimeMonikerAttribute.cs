namespace BitMono.Core.Attributes;

/// <summary>
/// Represents a mechanism that specifies the runtime moniker of the protection.
/// <remarks>i.e if you see this attribute on protection then only specified attributes are the supported runtime monikers.
/// If you don't see any of the attributes then it works everywhere, also, users will get message via <see cref="GetMessage"/></remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public abstract class RuntimeMonikerAttribute : Attribute
{
    protected RuntimeMonikerAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the name of the runtime moniker.
    /// </summary>
    public string Name { get; }

    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    public virtual string GetMessage()
    {
        return $"Intended for {Name} runtime";
    }
}