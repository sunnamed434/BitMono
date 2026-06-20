namespace BitMono.Core.Tests.Analyzing;

public class UnitySerializationCriticalAnalyzerTest
{
    private static UnitySerializationCriticalAnalyzer CreateAnalyzer(bool enabled = true)
    {
        return Setup.UnitySerializationCriticalAnalyzer(new ObfuscationSettings
        {
            UnitySerializedFieldsObfuscationExclude = enabled
        });
    }

    private static FieldDefinition Field(string typeName, string fieldName)
    {
        var module = ModuleDefinition.FromFile(typeof(UnityPlayer).Assembly.Location);
        return module.GetAllTypes().First(t => t.Name == typeName).Fields.First(f => f.Name == fieldName);
    }

    [Fact]
    public void PublicFieldOnMonoBehaviourIsCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityPublicField")).Should().BeFalse();
    }

    [Fact]
    public void SerializeFieldPrivateIsCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "unitySerialized")).Should().BeFalse();
    }

    [Fact]
    public void PlainPrivateFieldIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "unityPrivatePlain")).Should().BeTrue();
    }

    [Fact]
    public void StaticFieldIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityStaticField")).Should().BeTrue();
    }

    [Fact]
    public void ReadonlyFieldIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityReadonlyField")).Should().BeTrue();
    }

    [Fact]
    public void NonSerializedFieldIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityNonSerialized")).Should().BeTrue();
    }

    [Fact]
    public void PublicFieldOnNonUnityTypeIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityNonContainer), "PublicFieldOutsideUnity")).Should().BeTrue();
    }

    [Fact]
    public void NothingIsCriticalWhenDisabled()
    {
        CreateAnalyzer(enabled: false).NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityPublicField")).Should().BeTrue();
    }
}
