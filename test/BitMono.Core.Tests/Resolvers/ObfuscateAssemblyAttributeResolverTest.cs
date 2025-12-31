namespace BitMono.Core.Tests.Resolvers;

public class ObfuscateAssemblyAttributeResolverTest
{
    [Fact]
    public void WhenObfuscateAssemblyAttributeResolving_AndAssemblyHasObfuscateAssemblyAttributeIsPrivateTrue_ThenShouldBeTrue()
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscateAssemblyAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscateAssemblyAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(CustomAttributesInstance).Assembly.Location);

        var result = resolver.Resolve(module.Assembly);

        result
            .Should()
            .BeTrue();
    }
}