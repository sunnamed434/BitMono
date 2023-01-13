namespace BitMono.Core.Tests.Protecting.Analyzing;



public class ModelAttributeCriticalAnalyzerTest
{
    public static IEnumerable<object[]> GetAttributes()
    {
        yield return new object[] { nameof(SerializableAttribute), typeof(SerializableAttribute).Namespace };
        yield return new object[] { nameof(XmlAttributeAttribute), typeof(XmlAttributeAttribute).Namespace };
        yield return new object[] { nameof(XmlArrayItemAttribute), typeof(XmlArrayItemAttribute).Namespace };
        yield return new object[] { nameof(JsonPropertyAttribute), typeof(JsonPropertyAttribute).Namespace };
    }

    [Theory]
    [MemberData(nameof(GetAttributes))]
    public void WhenModelCriticalAnalyzing_AndModelIsCritical_ThenShouldBeFalse(string name, string @namespace)
    {
        var module = ModuleDefinition.FromBytes(Resources.HelloWorldLib);
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
        var configuration = Setup.Configuration(criticals);
        var criticalAnalyzer = Setup.ModelAttributeCriticalAnalyzer(configuration);
        var type = Setup.EmptyPublicType(module);
        var injector = Setup.MscorlibInjector();

        injector.InjectAttribute(module, @namespace, name, type);
        
        criticalAnalyzer.NotCriticalToMakeChanges(type).Should().BeFalse();
    }
}