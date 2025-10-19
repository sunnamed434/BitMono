namespace BitMono.Core.TestCases.CustomAttributes;

public class ObfuscationMethods
{
    public void NoObfuscationAttribute()
    {
    }
    [Obfuscation]
    public void VoidObfuscationAttribute()
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
    [Obfuscation(Feature = nameof(CallToCalli), Exclude = false)]
    public void ObfuscationAttributeFeatureCallToCalliExcludeFalse()
    {
    }   
}