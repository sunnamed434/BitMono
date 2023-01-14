namespace BitMono.Core.Tests.Protecting.Resolvers;

public class FeaturesData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { typeof(AntiDe4dot).GetName() };
        yield return new object[] { typeof(AntiILdasm).GetName() };
        yield return new object[] { typeof(AntiDecompiler).GetName() };
        yield return new object[] { typeof(CallToCalli).GetName() };
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class ObfuscationAttributeResolverTest
{
    [Theory]
    [ClassData(typeof(FeaturesData))]
    public void WhenObfuscationAttributeResolving_AndTypeHasComplexObfuscationAttributeWithExcludeFalseAttribute_ThenShouldBeFalse(string feature)
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var module = Setup.EmptyModule();
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var type = Setup.EmptyPublicType(module);
        var resolver = Setup.ObfuscationAttributeResolver(configuration);
        var attribute = Setup.CustomObfuscationAttribute(module, feature, type);
        var factory = module.CorLibTypeFactory;
        var argument = new CustomAttributeNamedArgument(CustomAttributeArgumentMemberType.Property,
            Setup.ObfuscationAttributeExcludePropertyName, factory.String,
            new CustomAttributeArgument(factory.Boolean, false));
        attribute.Signature.NamedArguments.Add(argument);
        
        var result = resolver.Resolve(feature, type);

        result.Should().BeFalse();
    }
    [Theory]
    [ClassData(typeof(FeaturesData))]
    public void WhenObfuscationAttributeResolving_AndTypeHasComplexObfuscationAttribute_ThenShouldBeTrue(string feature)
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var module = Setup.EmptyModule();
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var type = Setup.EmptyPublicType(module);
        var resolver = Setup.ObfuscationAttributeResolver(configuration);
        Setup.CustomObfuscationAttribute(module, feature, type);

        var result = resolver.Resolve(feature, type);

        result.Should().BeTrue();
    }
    [Fact]
    public void WhenObfuscationAttributeResolving_AndTypeHasObfuscationAttribute_ThenShouldBeTrue()
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var module = Setup.EmptyModule();
        var injector = Setup.MscorlibInjector();
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var type = Setup.EmptyPublicType(module);
        var resolver = Setup.ObfuscationAttributeResolver(configuration);
        injector.InjectAttribute(module, Setup.ObfuscationAttributeNamespace, Setup.ObfuscationAttributeName, type);

        var result = resolver.Resolve(type);

        result.Should().BeTrue();
    }
    [Fact]
    public void WhenObfuscationAttributeResolving_AndMethodHasObfuscationAttribute_ThenShouldBeTrue()
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var module = Setup.EmptyModule();
        var injector = Setup.MscorlibInjector();
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var method = Setup.EmptyPublicMethod(module);
        var resolver = Setup.ObfuscationAttributeResolver(configuration);
        injector.InjectAttribute(module, Setup.ObfuscationAttributeNamespace, Setup.ObfuscationAttributeName, method);

        var result = resolver.Resolve(method);

        result.Should().BeTrue();
    }
}