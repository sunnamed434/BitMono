namespace BitMono.Core.Tests.Analyzing;

public class SerializationCriticalAnalyzerTest
{
    private static SerializationCriticalAnalyzer CreateAnalyzer(bool enabled = true)
    {
        return Setup.SerializationCriticalAnalyzer(new ObfuscationSettings
        {
            SerializationMembersObfuscationExclude = enabled
        });
    }

    private static ModuleDefinition Module()
    {
        return ModuleDefinition.FromFile(typeof(SerializationDataContractModel).Assembly.Location);
    }

    private static FieldDefinition Field(ModuleDefinition module, string typeName, string fieldName)
    {
        return module.GetAllTypes().First(t => t.Name == typeName).Fields.First(f => f.Name == fieldName);
    }

    private static TypeDefinition Type(ModuleDefinition module, string typeName)
    {
        return module.GetAllTypes().First(t => t.Name == typeName);
    }

    [Fact]
    public void DataMemberFieldIsCritical()
    {
        var field = Field(Module(), nameof(SerializationDataContractModel), nameof(SerializationDataContractModel.Tagged));
        CreateAnalyzer().NotCriticalToMakeChanges(field).ShouldBeFalse();
    }

    [Fact]
    public void UntaggedFieldIsNotCritical()
    {
        var field = Field(Module(), nameof(SerializationDataContractModel), nameof(SerializationDataContractModel.Untagged));
        CreateAnalyzer().NotCriticalToMakeChanges(field).ShouldBeTrue();
    }

    [Fact]
    public void XmlElementFieldIsCritical()
    {
        var field = Field(Module(), nameof(SerializationXmlModel), nameof(SerializationXmlModel.XmlTagged));
        CreateAnalyzer().NotCriticalToMakeChanges(field).ShouldBeFalse();
    }

    [Fact]
    public void DataContractTypeIsCritical()
    {
        var type = Type(Module(), nameof(SerializationDataContractModel));
        CreateAnalyzer().NotCriticalToMakeChanges(type).ShouldBeFalse();
    }

    [Fact]
    public void ImplicitXmlSerializerFieldIsCritical()
    {
        var field = Field(Module(), nameof(SerializationImplicitXmlModel), nameof(SerializationImplicitXmlModel.ImplicitField));
        CreateAnalyzer().NotCriticalToMakeChanges(field).ShouldBeFalse();
    }

    [Fact]
    public void PlainFieldIsNotCritical()
    {
        var field = Field(Module(), nameof(SerializationPlainModel), nameof(SerializationPlainModel.PlainField));
        CreateAnalyzer().NotCriticalToMakeChanges(field).ShouldBeTrue();
    }

    [Fact]
    public void NothingIsCriticalWhenDisabled()
    {
        var field = Field(Module(), nameof(SerializationDataContractModel), nameof(SerializationDataContractModel.Tagged));
        CreateAnalyzer(enabled: false).NotCriticalToMakeChanges(field).ShouldBeTrue();
    }
}
