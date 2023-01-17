namespace BitMono.Core.Tests;

public static class Setup
{
    public static AttemptAttributeResolver AttemptAttributeResolver()
    {
        return new AttemptAttributeResolver(new CustomAttributeResolver());
    }
    public static ModelAttributeCriticalAnalyzer ModelAttributeCriticalAnalyzer(IOptions<Criticals> criticals)
    {
        return new ModelAttributeCriticalAnalyzer(criticals, AttemptAttributeResolver());
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
        return new ObfuscationAttributeResolver(obfuscation, AttemptAttributeResolver());
    }
    public static ObfuscateAssemblyAttributeResolver ObfuscateAssemblyAttributeResolver(
        IOptions<Obfuscation> obfuscation)
    {
        return new ObfuscateAssemblyAttributeResolver(obfuscation, AttemptAttributeResolver());
    }
}