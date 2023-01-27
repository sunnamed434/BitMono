namespace BitMono.Core.Tests.Protecting.Analyzing;

public class ReflectionCriticalAnalyzerTest
{
    [Fact]
    public void WhenReflectionCriticalAnalyzing_AndMethodUsesReflectionOfItSelf_ThenShouldBeFalse()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var obfuscation = new Obfuscation
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));
        
        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }
}