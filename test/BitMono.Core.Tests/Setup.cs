namespace BitMono.Core.Tests;

public static class Setup
{
    public static AttemptAttributeResolver AttemptAttributeResolver()
    {
        return new AttemptAttributeResolver(new CustomAttributeResolver());
    }
    public static ModelAttributeCriticalAnalyzer ModelAttributeCriticalAnalyzer(IOptions<CriticalsSettings> criticals)
    {
        return new ModelAttributeCriticalAnalyzer(criticals, AttemptAttributeResolver());
    }
    public static CriticalMethodsCriticalAnalyzer CriticalMethodsCriticalAnalyzer(IOptions<CriticalsSettings> criticals)
    {
        return new CriticalMethodsCriticalAnalyzer(criticals);
    }
    public static NoInliningMethodMemberResolver NoInliningMethodMemberResolver(IOptions<ObfuscationSettings> obfuscation)
    {
        return new NoInliningMethodMemberResolver(obfuscation);
    }
    public static ObfuscationAttributeResolver ObfuscationAttributeResolver(IOptions<ObfuscationSettings> obfuscation)
    {
        return new ObfuscationAttributeResolver(obfuscation, AttemptAttributeResolver());
    }
    public static ObfuscateAssemblyAttributeResolver ObfuscateAssemblyAttributeResolver(
        IOptions<ObfuscationSettings> obfuscation)
    {
        return new ObfuscateAssemblyAttributeResolver(obfuscation, AttemptAttributeResolver());
    }
    public static SerializableBitCriticalAnalyzer SerializableBitCriticalAnalyzer(IOptions<ObfuscationSettings> obfuscation)
    {
        return new SerializableBitCriticalAnalyzer(obfuscation);
    }
}