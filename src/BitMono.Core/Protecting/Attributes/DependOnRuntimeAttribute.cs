namespace BitMono.Core.Protecting.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DependOnRuntimeAttribute : Attribute
{
    public DependOnRuntimeAttribute(RuntimeMoniker runtimeMoniker)
    {
        RuntimeMoniker = runtimeMoniker;
    }

    public RuntimeMoniker RuntimeMoniker { get; }
}