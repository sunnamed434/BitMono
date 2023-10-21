namespace BitMono.Core.Attributes;

/// <summary>
/// Represents a mechanism that specifies the runtime moniker of the protection as a <b>Mono</b>.
/// <remarks>i.e if you see this attribute on protection then only specified attributes are the supported runtime monikers for the protection.
/// If you don't see any of the attributes then it works everywhere, also, users will get a message via <see cref="RuntimeMonikerAttribute.GetMessage"/></remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RuntimeMonikerMonoAttribute : RuntimeMonikerAttribute
{
    public RuntimeMonikerMonoAttribute() : base(KnownRuntimeMonikers.Mono)
    {
    }
}