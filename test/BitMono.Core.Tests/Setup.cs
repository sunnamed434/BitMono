namespace BitMono.Core.Tests;

public static class Setup
{
    public static TestBitMonoCriticalsConfiguration Configuration(object value)
    {
        var json = JsonConvert.SerializeObject(value);
        return new TestBitMonoCriticalsConfiguration(json);
    }
    public static TypeDefinition EmptyPublicType(string @namespace, string name)
    {
        return new TypeDefinition(@namespace, name, TypeAttributes.Public);
    }
    public static TypeDefinition EmptyPublicType()
    {
        return EmptyPublicType(string.Empty, Guid.NewGuid().ToString());
    }
    public static TypeDefinition EmptyPublicType(ModuleDefinition injectTo, string @namespace, string name)
    {
        var type = EmptyPublicType(@namespace, name);
        injectTo.TopLevelTypes.Add(type);
        return type;
    }
    public static TypeDefinition EmptyPublicType(ModuleDefinition injectTo)
    {
        var type = EmptyPublicType();
        injectTo.TopLevelTypes.Add(type);
        return type;
    }
    public static MethodDefinition EmptyPublicMethod(ModuleDefinition module, string name)
    {
        var factory = module.CorLibTypeFactory;
        return new MethodDefinition(name, MethodAttributes.Public, MethodSignature.CreateInstance(factory.Void));
    }
    public static MethodDefinition EmptyPublicMethod(ModuleDefinition module)
    {
        return EmptyPublicMethod(module, Guid.NewGuid().ToString());
    }
    public static MscorlibInjector MscorlibInjector()
    {
        return new MscorlibInjector();
    }

    public static ModuleDefinition EmptyModule()
    {
        return new ModuleDefinition("TestModule");
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
}