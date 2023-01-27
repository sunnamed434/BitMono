namespace BitMono.Core.Protecting.Attributes;

[Flags]
public enum MemberInclusionFlags
{
    SpecialRuntime = 0x1,
    Model = 0x2,
    Reflection = 0x4,
}