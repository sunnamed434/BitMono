namespace BitMono.Core.TestCases.CustomAttributes;

public class MembersWithCustomAttribute
{
    public void VoidMethod()
    {
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void NoInliningMethod()
    {
    }
    [Obfuscation(Feature = nameof(CallToCalli))]
    public void ObfuscationAttributeFeatureCallToCalli()
    {
    }
    [Obfuscation(Feature = nameof(CallToCalli), Exclude = true)]
    public void ObfuscationAttributeFeatureCallToCalliExcludeTrue()
    {
    }
    [Obfuscation(Feature = nameof(CallToCalli), Exclude = true)]
    public void ObfuscationAttributeFeatureCallToCalliExcludeFalse()
    {
    }
}