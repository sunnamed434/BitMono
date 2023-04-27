namespace BitMono.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RuntimeMonikerNETCore : RuntimeMonikerAttribute
{
    public RuntimeMonikerNETCore() : base(KnownRuntimeMonikers.NETCore)
    {
    }
}