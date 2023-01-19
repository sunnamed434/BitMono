namespace BitMono.Core.TestCases.CustomAttributes;

public class ObfuscationTypes
{
    public class NoObfuscationAttribute
    {
    }
    [Obfuscation]
    public class VoidObfuscationAttribute
    {
    }
    [Obfuscation(Feature = nameof(CallToCalli))]
    public class ObfuscationAttributeCallToCalli
    {
    }
    [Obfuscation(Feature = nameof(CallToCalli), Exclude = true)]
    public class ObfuscationAttributeCallToCalliWithExcludeTrue
    {
    }
    [Obfuscation(Feature = nameof(CallToCalli), Exclude = false)]
    public class ObfuscationAttributeCallToCalliWithExcludeFalse
    {
    }
}