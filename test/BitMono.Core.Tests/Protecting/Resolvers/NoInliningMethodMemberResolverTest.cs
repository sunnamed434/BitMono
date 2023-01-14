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
        var module = Setup.EmptyModule();
        var method = Setup.EmptyPublicMethod(module);
        method.NoInlining = true;
        
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
        var module = Setup.EmptyModule();
        var method = Setup.EmptyPublicMethod(module);
        
        var result = resolver.Resolve(null, method);

        result.Should().BeTrue();
    }
}