namespace BitMono.Core.Tests.Protecting.Analyzing;

public class AttributesData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { nameof(SerializableAttribute), typeof(SerializableAttribute).Namespace };
        yield return new object[] { nameof(XmlAttributeAttribute), typeof(XmlAttributeAttribute).Namespace };
        yield return new object[] { nameof(XmlArrayItemAttribute), typeof(XmlArrayItemAttribute).Namespace };
        yield return new object[] { nameof(JsonPropertyAttribute), typeof(JsonPropertyAttribute).Namespace };
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class ModelAttributeCriticalAnalyzerTest
{
    [Theory]
    [ClassData(typeof(AttributesData))]
    public void WhenModelCriticalAnalyzing_AndModelIsCritical_ThenShouldBeFalse(string name, string @namespace)
    {
        var module = Setup.EmptyModule();
        var criticals = new Criticals()
        {
            UseCriticalModelAttributes = true,
            CriticalModelAttributes = new List<CriticalAttribute>
            {
                new CriticalAttribute
                {
                    Namespace = @namespace,
                    Name = name
                },
            }
        };
        var configuration = Setup.CriticalsConfiguration(criticals);
        var criticalAnalyzer = Setup.ModelAttributeCriticalAnalyzer(configuration);
        var type = Setup.EmptyPublicType(module);
        var injector = Setup.MscorlibInjector();

        injector.InjectAttribute(module, @namespace, name, type);
        var result = criticalAnalyzer.NotCriticalToMakeChanges(type);
        
        result.Should().BeFalse();
    }
    [Theory]
    [ClassData(typeof(AttributesData))]
    public void WhenModelCriticalAnalyzing_AndModelIsNotCritical_ThenShouldBeTrue(string name, string @namespace)
    {
        var module = Setup.EmptyModule();
        var criticals = new Criticals()
        {
            UseCriticalModelAttributes = true,
            CriticalModelAttributes = new List<CriticalAttribute>
            {
                new CriticalAttribute
                {
                    Namespace = @namespace,
                    Name = name
                },
            }
        };
        var configuration = Setup.CriticalsConfiguration(criticals);
        var criticalAnalyzer = Setup.ModelAttributeCriticalAnalyzer(configuration);
        var type = Setup.EmptyPublicType(module);

        var result = criticalAnalyzer.NotCriticalToMakeChanges(type);

        result.Should().BeTrue();
    }
}