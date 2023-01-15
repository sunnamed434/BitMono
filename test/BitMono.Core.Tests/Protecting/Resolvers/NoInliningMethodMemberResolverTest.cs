namespace BitMono.Core.Tests.Protecting.Resolvers;

public class NoInliningMethodMemberResolverTest
{
    [Fact]
    public void WhenNoInliningMethodResolving_AndMethodHasInliningBit_ThenShouldBeFalse()
    {
        var obfuscation = new Obfuscation
        {
            NoInliningMethodObfuscationExclude = true,
        };
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var resolver = Setup.NoInliningMethodMemberResolver(configuration);
        var module = ModuleDefinition.FromFile(typeof(MembersWithCustomAttribute).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(MembersWithCustomAttribute));
        var method = type.Methods.First(m => m.Name == nameof(MembersWithCustomAttribute.NoInliningMethod));
        
        var result = resolver.Resolve(null, method);

        result.Should().BeFalse();
    }
    [Fact]
    public void WhenNoInliningMethodResolving_AndMethodHasNoInliningBit_ThenShouldBeTrue()
    {
        var obfuscation = new Obfuscation
        {
            NoInliningMethodObfuscationExclude = true,
        };
        var configuration = Setup.ObfuscationConfiguration(obfuscation);
        var resolver = Setup.NoInliningMethodMemberResolver(configuration);
        var module = ModuleDefinition.FromFile(typeof(MembersWithCustomAttribute).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(MembersWithCustomAttribute));
        var method = type.Methods.First(m => m.Name == nameof(MembersWithCustomAttribute.VoidMethod));
        
        var result = resolver.Resolve(null, method);

        result.Should().BeTrue();
    }
}