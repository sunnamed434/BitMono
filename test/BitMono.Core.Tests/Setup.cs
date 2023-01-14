namespace BitMono.Core.Tests;

public static class Setup
{
    public static readonly string ObfuscationAttributeNamespace = typeof(ObfuscationAttribute).Namespace; 
    public static readonly string ObfuscationAttributeName = nameof(ObfuscationAttribute);
    public static readonly string ObfuscationAttributeFeaturePropertyName = nameof(ObfuscationAttribute.Feature);
    public static readonly string ObfuscationAttributeExcludePropertyName = nameof(ObfuscationAttribute.Exclude);
    
    public static TestBitMonoCriticalsConfiguration CriticalsConfiguration(Criticals model)
    {
        return new TestBitMonoCriticalsConfiguration(JsonConvert.SerializeObject(model));
    }
    public static TestBitMonoObfuscationConfiguration ObfuscationConfiguration(Obfuscation model)
    {
        return new TestBitMonoObfuscationConfiguration(JsonConvert.SerializeObject(model));
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
    public static CustomAttribute CustomObfuscationAttribute(ModuleDefinition module)
    {
        var factory = module.CorLibTypeFactory;
        var ctor = factory.CorLibScope
            .CreateTypeReference(ObfuscationAttributeNamespace, ObfuscationAttributeName)
            .CreateMemberReference(".ctor", MethodSignature.CreateInstance(factory.Void))
            .ImportWith(module.DefaultImporter);
        return new CustomAttribute(ctor);
    }
    public static CustomAttribute CustomObfuscationAttribute(ModuleDefinition module, string feature)
    {
        var factory = module.CorLibTypeFactory;
        var argument = new CustomAttributeNamedArgument(CustomAttributeArgumentMemberType.Property,
            ObfuscationAttributeFeaturePropertyName, factory.String,
            new CustomAttributeArgument(factory.String, feature));
        var attribute = CustomObfuscationAttribute(module);
        attribute.Signature.NamedArguments.Add(argument);
        return attribute;
    }
    public static CustomAttribute CustomObfuscationAttribute(ModuleDefinition module, string feature, IHasCustomAttribute injectTo)
    {
        var attribute = CustomObfuscationAttribute(module, feature);
        injectTo.CustomAttributes.Add(attribute);
        return attribute;
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