namespace BitMono.Core.Tests;

public static class Setup
{
    public static TestBitMonoCriticalsConfiguration CriticalsConfiguration(Criticals model)
    {
        return new TestBitMonoCriticalsConfiguration(JsonConvert.SerializeObject(model));
    }
    public static TestBitMonoObfuscationConfiguration ObfuscationConfiguration(Obfuscation model)
    {
        return new TestBitMonoObfuscationConfiguration(JsonConvert.SerializeObject(model));
    }
    public static ModelAttributeCriticalAnalyzer ModelAttributeCriticalAnalyzer(IBitMonoCriticalsConfiguration configuration)
    {
        var attemptAttributeResolver = new AttemptAttributeResolver(new CustomAttributeResolver());
        return new ModelAttributeCriticalAnalyzer(configuration, attemptAttributeResolver);
    }
    public static CriticalMethodsCriticalAnalyzer CriticalMethodsCriticalAnalyzer(
        IBitMonoCriticalsConfiguration configuration)
    {
        return new CriticalMethodsCriticalAnalyzer(configuration);
    }
    public static NoInliningMethodMemberResolver NoInliningMethodMemberResolver(
        IBitMonoObfuscationConfiguration configuration)
    {
        return new NoInliningMethodMemberResolver(configuration);
    }
    public static ObfuscationAttributeResolver ObfuscationAttributeResolver(
        IBitMonoObfuscationConfiguration configuration)
    {
        return new ObfuscationAttributeResolver(configuration, new AttemptAttributeResolver(new CustomAttributeResolver()));
    }
}