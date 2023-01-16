namespace BitMono.Core.Tests.Protecting.Resolvers;

public class ObfuscationAttributeResolverTest
{
    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void WhenObfuscationAttributeResolving_AndTypeHasComplexObfuscationAttributeWithExcludeFalse_ThenShouldBeFalse(string feature)
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var resolver = Setup.ObfuscationAttributeResolver(configuration);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeCallToCalliWithExcludeFalse));
        
        var result = resolver.Resolve(feature, type);

        result.Should().BeFalse();
    }
    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void WhenObfuscationAttributeResolving_AndTypeHasObfuscationAttributeCallToCalli_ThenShouldBeTrue(string feature)
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var resolver = Setup.ObfuscationAttributeResolver(configuration);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeCallToCalli));

        var result = resolver.Resolve(feature, type);

        result.Should().BeTrue();
    }
    [Fact]
    public void WhenObfuscationAttributeResolving_AndTypeHasVoidObfuscationAttribute_ThenShouldBeTrue()
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var resolver = Setup.ObfuscationAttributeResolver(configuration);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.VoidObfuscationAttribute));

        var result = resolver.Resolve(type);

        result.Should().BeTrue();
    }
    [Fact]
    public void WhenObfuscationAttributeResolving_AndMethodHasVoidObfuscationAttribute_ThenShouldBeTrue()
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var resolver = Setup.ObfuscationAttributeResolver(configuration);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(n => n.Name == nameof(ObfuscationMethods));
        var method = type.Methods.First(m => m.Name == nameof(ObfuscationMethods.VoidObfuscationAttribute));

        var result = resolver.Resolve(method);

        result.Should().BeTrue();
    }
}