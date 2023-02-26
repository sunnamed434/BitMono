namespace BitMono.Core.Tests.Resolvers;

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
        var options = Options.Create(obfuscation);
        var resolver = Setup.ObfuscationAttributeResolver(options);
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
        var options = Options.Create(obfuscation);
        var resolver = Setup.ObfuscationAttributeResolver(options);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n => n.Name == nameof(ObfuscationTypes.ObfuscationAttributeCallToCalli));

        var result = resolver.Resolve(feature, type);

        result.Should().BeTrue();
    }
    [Fact]
    public void WhenObfuscationAttributeResolving_AndTypeHasVoidObfuscationAttribute_ThenShouldBeFalse()
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var options = Options.Create(obfuscation);
        var resolver = Setup.ObfuscationAttributeResolver(options);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n => n.Name == nameof(ObfuscationTypes.VoidObfuscationAttribute));

        var result = resolver.Resolve(string.Empty, type);

        result.Should().BeFalse();
    }
    [Fact]
    public void WhenObfuscationAttributeResolving_AndMethodHasVoidObfuscationAttribute_ThenShouldBeFalse()
    {
        var obfuscation = new Obfuscation
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var options = Options.Create(obfuscation);
        var resolver = Setup.ObfuscationAttributeResolver(options);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(n => n.Name == nameof(ObfuscationMethods));
        var method = type.Methods.First(m => m.Name == nameof(ObfuscationMethods.VoidObfuscationAttribute));

        var result = resolver.Resolve(string.Empty, method);

        result.Should().BeFalse();
    }
}