namespace BitMono.Core.Tests.Analyzing;

public class MethodsData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { nameof(CriticalMethods.Update) };
        yield return new object[] { nameof(CriticalMethods.LateUpdate) };
        yield return new object[] { nameof(CriticalMethods.FixedUpdate) };
        yield return new object[] { nameof(CriticalMethods.OnDrawGizmos) };
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class CriticalMethodsCriticalAnalyzerTest
{
    [Theory]
    [ClassData(typeof(MethodsData))]
    public void WhenMethodCriticalAnalyzing_AndMethodIsCritical_ThenShouldBeFalse(string methodName)
    {
        var criticals = new CriticalsSettings
        {
            UseCriticalMethods = true,
            CriticalMethods = new List<string>
            {
                methodName
            }
        };
        var criticalAnalyzer = Setup.CriticalMethodsCriticalAnalyzer(criticals);
        var module = ModuleDefinition.FromFile(typeof(CriticalMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(CriticalMethods));
        var method = type.Methods.First(m => m.Name == methodName);

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result
            .Should()
            .BeFalse();
    }
    [Theory]
    [ClassData(typeof(MethodsData))]
    public void WhenMethodCriticalAnalyzing_AndMethodIsNotCritical_ThenShouldBeTrue(string methodName)
    {
        var criticals = new CriticalsSettings
        {
            UseCriticalMethods = true,
            CriticalMethods = new List<string>
            {
                methodName
            }
        };
        var criticalAnalyzer = Setup.CriticalMethodsCriticalAnalyzer(criticals);
        var module = ModuleDefinition.FromFile(typeof(CriticalMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(CriticalMethods));
        var method = type.Methods.First(m => m.Name == nameof(CriticalMethods.VoidMethod));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result
            .Should()
            .BeTrue();
    }
}