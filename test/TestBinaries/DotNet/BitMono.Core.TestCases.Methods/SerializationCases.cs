using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BitMono.Core.TestCases.Methods;

// Fixtures for SerializationCriticalAnalyzer. Real compiled IL so the attributes and the
// `new XmlSerializer(typeof(T))` call site are exactly what the compiler emits.

[DataContract]
public class SerializationDataContractModel
{
    [DataMember] public int Tagged;
    public int Untagged;
}

[XmlRoot("root")]
public class SerializationXmlModel
{
    [XmlElement] public string XmlTagged;
    public int Plain;
}

public class SerializationImplicitXmlModel
{
    public int ImplicitField;
    public int ImplicitProperty { get; set; }
}

public class SerializationXmlSerializerUser
{
    public void Use()
    {
        var serializer = new XmlSerializer(typeof(SerializationImplicitXmlModel));
        _ = serializer;
    }
}

public class SerializationPlainModel
{
    public int PlainField;
}
