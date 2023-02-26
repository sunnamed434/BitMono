namespace BitMono.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DoNotResolveAttribute : Attribute
{
    public DoNotResolveAttribute(MemberInclusionFlags memberInclusion)
    {
        MemberInclusion = memberInclusion;
    }

    public MemberInclusionFlags MemberInclusion { get; }
}