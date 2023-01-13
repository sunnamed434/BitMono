namespace BitMono.Core.Tests;

public static class Setup
{
    public static TestBitMonoCriticalsConfiguration Configuration(object value)
    {
        var json = JsonConvert.SerializeObject(value);
        return new TestBitMonoCriticalsConfiguration(json);
    }
    public static TypeDefinition EmptyPublicType()
    {
        return new TypeDefinition(string.Empty, Guid.NewGuid().ToString(), TypeAttributes.Public);
    }
    public static TypeDefinition EmptyPublicType(ModuleDefinition injectTo)
    {
        var type = EmptyPublicType();
        injectTo.TopLevelTypes.Add(type);
        return type;
    }
    public static MscorlibInjector MscorlibInjector()
    {
        return new MscorlibInjector();
    }
    public static ModelAttributeCriticalAnalyzer ModelAttributeCriticalAnalyzer(IBitMonoCriticalsConfiguration configuration)
    {
        var attemptAttributeResolver = new AttemptAttributeResolver(new CustomAttributeResolver());
        return new ModelAttributeCriticalAnalyzer(configuration, attemptAttributeResolver);
    }
}