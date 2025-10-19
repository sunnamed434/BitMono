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
    [Obfuscation(Feature = nameof(CallToCalli), Exclude = true, ApplyToMembers = false)]
    public class ObfuscationAttributeCallToCalliWithApplyToMembersFalse
    {
    }
    [Obfuscation(Feature = nameof(CallToCalli), Exclude = true, StripAfterObfuscation = false)]
    public class ObfuscationAttributeCallToCalliWithStripAfterObfuscationFalse
    {
    }
    [Obfuscation(Feature = "DifferentFeature", Exclude = true)]
    public class ObfuscationAttributeDifferentFeature
    {
    }
    [Obfuscation(Feature = "", Exclude = true)]
    public class ObfuscationAttributeEmptyFeature
    {
    }
    [Obfuscation(Exclude = true)]
    public class ObfuscationAttributeNoFeature
    {
    }
    [Obfuscation(Feature = nameof(CallToCalli), Exclude = true, ApplyToMembers = true, StripAfterObfuscation = true)]
    public class ObfuscationAttributeAllPropertiesTrue
    {
    }
}