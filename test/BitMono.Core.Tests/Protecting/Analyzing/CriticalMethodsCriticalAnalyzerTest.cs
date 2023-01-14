namespace BitMono.Core.Tests.Protecting.Analyzing;

public class MethodsData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "Update" };
        yield return new object[] { "LateUpdate" };
        yield return new object[] { "FixedUpdate" };
        yield return new object[] { "OnDrawGizmos" };
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
    public void WhenMethodIsCritical_AndMethodIsCritical_ThenShouldBeFalse(string methodName)
    {
        var module = Setup.EmptyModule();
        var criticals = new Criticals
        {
            UseCriticalMethods = true,
            CriticalMethods = new List<string>
            {
                methodName
            }
        };
        var configuration = Setup.Configuration(criticals);
        var criticalAnalyzer = Setup.CriticalMethodsCriticalAnalyzer(configuration);
        
        var method = Setup.EmptyPublicMethod(module, methodName);
        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Theory]
    [ClassData(typeof(MethodsData))]
    public void WhenMethodIsNotCritical_AndMethodIsNotCritical_ThenShouldBeTrue(string methodName)
    {
        var module = Setup.EmptyModule();
        var criticals = new Criticals
        {
            UseCriticalMethods = true,
            CriticalMethods = new List<string>
            {
                methodName
            }
        };
        var configuration = Setup.Configuration(criticals);
        var criticalAnalyzer = Setup.CriticalMethodsCriticalAnalyzer(configuration);
        
        var method = Setup.EmptyPublicMethod(module);
        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeTrue();
    }
}