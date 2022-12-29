namespace BitMono.Core.Protecting.Attributes;

[Flags]
public enum Members
{
    None = 0,
    SpecialRuntime = 0x1,
    Model = 0x2,
}