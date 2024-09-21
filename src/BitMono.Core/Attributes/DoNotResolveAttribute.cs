namespace BitMono.Core.Attributes;

/// <summary>
/// Represents a sort logic which doesn't include specified <see cref="MemberInclusionFlags"/> in <see cref="Protection"/> <see cref="ProtectionParameters.Members"/>.
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