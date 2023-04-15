namespace BitMono.Core.Tests.Resolvers;

public class NoInliningMethodMemberResolverTest
{
    [Fact]
    public void WhenNoInliningMethodResolving_AndMethodHasInliningBit_ThenShouldBeFalse()
    {
        var obfuscation = new ObfuscationSettings
        {
            NoInliningMethodObfuscationExclude = true,
        };
        var options = Options.Create(obfuscation);
        var resolver = Setup.NoInliningMethodMemberResolver(options);
        var module = ModuleDefinition.FromFile(typeof(NoInliningMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(NoInliningMethods));
        var method = type.Methods.First(m => m.Name == nameof(NoInliningMethods.NoInliningMethod));

        var result = resolver.Resolve(null, method);

        result
            .Should()
            .BeFalse();
    }
    [Fact]
    public void WhenNoInliningMethodResolving_AndMethodHasNoInliningBit_ThenShouldBeTrue()
    {
        var obfuscation = new ObfuscationSettings
        {
            NoInliningMethodObfuscationExclude = true,
        };
        var options = Options.Create(obfuscation);
        var resolver = Setup.NoInliningMethodMemberResolver(options);
        var module = ModuleDefinition.FromFile(typeof(NoInliningMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(NoInliningMethods));
        var method = type.Methods.First(m => m.Name == nameof(NoInliningMethods.VoidMethod));

        var result = resolver.Resolve(null, method);

        result
            .Should()
            .BeTrue();
    }
}