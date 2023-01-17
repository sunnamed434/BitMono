namespace BitMono.Core.Tests;

public static class Setup
{
    public static ModelAttributeCriticalAnalyzer ModelAttributeCriticalAnalyzer(IOptions<Criticals> criticals)
    {
        var attemptAttributeResolver = new AttemptAttributeResolver(new CustomAttributeResolver());
        return new ModelAttributeCriticalAnalyzer(criticals, attemptAttributeResolver);
    }
    public static CriticalMethodsCriticalAnalyzer CriticalMethodsCriticalAnalyzer(IOptions<Criticals> criticals)
    {
        return new CriticalMethodsCriticalAnalyzer(criticals);
    }
    public static NoInliningMethodMemberResolver NoInliningMethodMemberResolver(IOptions<Obfuscation> obfuscation)
    {
        return new NoInliningMethodMemberResolver(obfuscation);
    }
    public static ObfuscationAttributeResolver ObfuscationAttributeResolver(IOptions<Obfuscation> obfuscation)
    {
        return new ObfuscationAttributeResolver(obfuscation, new AttemptAttributeResolver(new CustomAttributeResolver()));
    }
}