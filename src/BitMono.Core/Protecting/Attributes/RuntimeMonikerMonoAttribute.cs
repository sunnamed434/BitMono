namespace BitMono.Core.Protecting.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RuntimeMonikerMonoAttribute : RuntimeMonikerAttribute
{
    public RuntimeMonikerMonoAttribute() : base(KnownRuntimeMonikers.Mono)
    {
    }
}