namespace BitMono.Core.Tests;

public static class Setup
{
    public static ModelAttributeCriticalAnalyzer ModelAttributeCriticalAnalyzer(CriticalsSettings criticals)
    {
        return new ModelAttributeCriticalAnalyzer(criticals);
    }
    public static CriticalMethodsCriticalAnalyzer CriticalMethodsCriticalAnalyzer(CriticalsSettings criticals)
    {
        return new CriticalMethodsCriticalAnalyzer(criticals);
    }
    public static CriticalMethodsStartsWithAnalyzer CriticalMethodsStartsWithCriticalAnalyzer(CriticalsSettings criticals)
    {
        return new CriticalMethodsStartsWithAnalyzer(criticals);
    }
    public static NoInliningMethodMemberResolver NoInliningMethodMemberResolver(ObfuscationSettings obfuscation)
    {
        return new NoInliningMethodMemberResolver(obfuscation);
    }
    public static ObfuscationAttributeResolver ObfuscationAttributeResolver(ObfuscationSettings obfuscation)
    {
        return new ObfuscationAttributeResolver(obfuscation);
    }
    public static ObfuscateAssemblyAttributeResolver ObfuscateAssemblyAttributeResolver(ObfuscationSettings obfuscation)
    {
        return new ObfuscateAssemblyAttributeResolver(obfuscation);
    }
    public static SerializableBitCriticalAnalyzer SerializableBitCriticalAnalyzer(ObfuscationSettings obfuscation)
    {
        return new SerializableBitCriticalAnalyzer(obfuscation);
    }
    public static ReflectionCriticalAnalyzer ReflectionCriticalAnalyzer(ObfuscationSettings obfuscation)
    {
        return new ReflectionCriticalAnalyzer(obfuscation);
    }
}
