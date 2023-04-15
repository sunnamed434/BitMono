namespace BitMono.Core.Tests.Analyzing;

public class ReflectionCriticalAnalyzerTest
{
    [Fact]
    public void WhenReflectionCriticalAnalyzing_AndMethodUsesReflectionOfItSelf_ThenShouldBeFalse()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result
            .Should()
            .BeFalse();
    }
    [Fact]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public void WhenReflectionCriticalAnalyzing_AndMethodUses2DifferentReflectionAnd1OnItSelf_ThenShouldBeFalseAndCountOfCachedMethodsShouldBe1AndMethodNameShouldBeEqualToSelf()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.Uses3Reflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result
            .Should()
            .BeFalse();
        criticalAnalyzer.CachedMethods.Count
            .Should()
            .Be(1);
        criticalAnalyzer.CachedMethods.First().Name.Value
            .Should()
            .Be(method.Name);
    }
}