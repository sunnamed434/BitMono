namespace BitMono.Core.Attributes;

/// <summary>
/// Represents a sort logic which doesn't includes specified <see cref="MemberInclusionFlags"/> in arguments of Protection (i.e Members).
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DoNotResolveAttribute : Attribute
{
    public DoNotResolveAttribute(MemberInclusionFlags memberInclusion)
    {
        MemberInclusion = memberInclusion;
    }

    public MemberInclusionFlags MemberInclusion { get; }
}