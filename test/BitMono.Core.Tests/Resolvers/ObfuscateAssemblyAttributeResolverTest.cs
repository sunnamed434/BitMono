namespace BitMono.Core.Tests.Resolvers;

public class ObfuscateAssemblyAttributeResolverTest
{
    [Fact]
    public void WhenObfuscateAssemblyAttributeResolving_AndAssemblyHasObfuscateAssemblyAttributeIsPrivateTrue_ThenShouldBeTrue()
    {
        var obfuscation = new Obfuscation
        {
            ObfuscateAssemblyAttributeObfuscationExclude = true,
        };
        var options = Options.Create(obfuscation);
        var resolver = Setup.ObfuscateAssemblyAttributeResolver(options);
        var module = ModuleDefinition.FromFile(typeof(CustomAttributesInstance).Assembly.Location);

        var result = resolver.Resolve(module.Assembly);

        result
            .Should()
            .BeTrue();
    }
}