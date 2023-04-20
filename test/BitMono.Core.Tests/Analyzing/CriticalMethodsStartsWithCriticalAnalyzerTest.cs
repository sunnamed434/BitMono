namespace BitMono.Core.Tests.Analyzing;

public class MethodsStartsWithData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { nameof(CriticalMethods.OV_method) };
        yield return new object[] { nameof(CriticalMethods.OV_override) };
        yield return new object[] { nameof(CriticalMethods.OV_) };
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class CriticalMethodsStartsWithCriticalAnalyzerTest
{
    [Theory]
    [ClassData(typeof(MethodsStartsWithData))]
    public void WhenMethodCriticalAnalyzing_AndMethodIsCritical_ThenShouldBeFalse(string methodName)
    {
        var criticals = new CriticalsSettings
        {
            UseCriticalMethodsStartsWith = true,
            CriticalMethodsStartsWith = new List<string>
            {
                methodName
            }
        };
        var options = Options.Create(criticals);
        var criticalAnalyzer = Setup.CriticalMethodsStartsWithCriticalAnalyzer(options);
        var module = ModuleDefinition.FromFile(typeof(CriticalMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(CriticalMethods));
        var method = type.Methods.First(m => m.Name == methodName);

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result
            .Should()
            .BeFalse();
    }
    [Theory]
    [ClassData(typeof(MethodsStartsWithData))]
    public void WhenMethodCriticalAnalyzing_AndMethodIsNotCritical_ThenShouldBeTrue(string methodName)
    {
        var criticals = new CriticalsSettings
        {
            UseCriticalMethodsStartsWith = true,
            CriticalMethodsStartsWith = new List<string>
            {
                methodName
            }
        };
        var options = Options.Create(criticals);
        var criticalAnalyzer = Setup.CriticalMethodsStartsWithCriticalAnalyzer(options);
        var module = ModuleDefinition.FromFile(typeof(CriticalMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(CriticalMethods));
        var method = type.Methods.First(m => m.Name == nameof(CriticalMethods.VoidMethod));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result
            .Should()
            .BeTrue();
    }
}