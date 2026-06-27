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
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityPublicField")).ShouldBeFalse();
    }

    [Fact]
    public void SerializeFieldPrivateIsCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "unitySerialized")).ShouldBeFalse();
    }

    [Fact]
    public void PlainPrivateFieldIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "unityPrivatePlain")).ShouldBeTrue();
    }

    [Fact]
    public void StaticFieldIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityStaticField")).ShouldBeTrue();
    }

    [Fact]
    public void ReadonlyFieldIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityReadonlyField")).ShouldBeTrue();
    }

    [Fact]
    public void NonSerializedFieldIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityNonSerialized")).ShouldBeTrue();
    }

    [Fact]
    public void PublicFieldOnNonUnityTypeIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Field(nameof(UnityNonContainer), "PublicFieldOutsideUnity")).ShouldBeTrue();
    }

    [Fact]
    public void NothingIsCriticalWhenDisabled()
    {
        CreateAnalyzer(enabled: false).NotCriticalToMakeChanges(Field(nameof(UnityPlayer), "UnityPublicField")).ShouldBeTrue();
    }
}
