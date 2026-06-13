using System.IO;
using System.Text;

namespace BitMono.Core.Analyzing.Baml;

internal sealed class BamlDocument : List<BamlRecord>
{
    public struct BamlVersion
    {
        public ushort Major;
        public ushort Minor;
    }

    public string Signature { get; set; }
    public BamlVersion ReaderVersion;
    public BamlVersion UpdaterVersion;
    public BamlVersion WriterVersion;
    public string DocumentName { get; set; }
}

internal static class BamlReader
{
    public static BamlDocument ReadDocument(Stream stream)
    {
        var document = new BamlDocument();
        var reader = new BamlBinaryReader(stream);
        {
            var unicode = new BinaryReader(stream, Encoding.Unicode);
            var length = unicode.ReadUInt32();
            document.Signature = new string(unicode.ReadChars((int)(length >> 1)));
            unicode.ReadBytes((int)(((length + 3) & ~3) - length));
        }
        if (document.Signature != "MSBAML")
        {
            throw new NotSupportedException("Not a BAML stream.");
        }
        document.ReaderVersion = new BamlDocument.BamlVersion { Major = reader.ReadUInt16(), Minor = reader.ReadUInt16() };
        document.UpdaterVersion = new BamlDocument.BamlVersion { Major = reader.ReadUInt16(), Minor = reader.ReadUInt16() };
        document.WriterVersion = new BamlDocument.BamlVersion { Major = reader.ReadUInt16(), Minor = reader.ReadUInt16() };

        var records = new Dictionary<long, BamlRecord>();
        while (stream.Position < stream.Length)
        {
            var position = stream.Position;
            var type = (BamlRecordType)reader.ReadByte();
            var record = Create(type);
            record.Position = position;
            record.Read(reader);
            document.Add(record);
            records[position] = record;
        }
        for (var i = 0; i < document.Count; i++)
        {
            if (document[i] is IBamlDeferRecord defer)
            {
                defer.ReadDefer(document, i, p => records[p]);
            }
        }
        return document;
    }

    private static BamlRecord Create(BamlRecordType type)
    {
        return type switch
        {
            BamlRecordType.AssemblyInfo => new AssemblyInfoRecord(),
            BamlRecordType.AttributeInfo => new AttributeInfoRecord(),
            BamlRecordType.ConstructorParametersStart => new ConstructorParametersStartRecord(),
            BamlRecordType.ConstructorParametersEnd => new ConstructorParametersEndRecord(),
            BamlRecordType.ConstructorParameterType => new ConstructorParameterTypeRecord(),
            BamlRecordType.ConnectionId => new ConnectionIdRecord(),
            BamlRecordType.ContentProperty => new ContentPropertyRecord(),
            BamlRecordType.DefAttribute => new DefAttributeRecord(),
            BamlRecordType.DefAttributeKeyString => new DefAttributeKeyStringRecord(),
            BamlRecordType.DefAttributeKeyType => new DefAttributeKeyTypeRecord(),
            BamlRecordType.DeferableContentStart => new DeferableContentStartRecord(),
            BamlRecordType.DocumentEnd => new DocumentEndRecord(),
            BamlRecordType.DocumentStart => new DocumentStartRecord(),
            BamlRecordType.ElementEnd => new ElementEndRecord(),
            BamlRecordType.ElementStart => new ElementStartRecord(),
            BamlRecordType.KeyElementEnd => new KeyElementEndRecord(),
            BamlRecordType.KeyElementStart => new KeyElementStartRecord(),
            BamlRecordType.LineNumberAndPosition => new LineNumberAndPositionRecord(),
            BamlRecordType.LinePosition => new LinePositionRecord(),
            BamlRecordType.LiteralContent => new LiteralContentRecord(),
            BamlRecordType.NamedElementStart => new NamedElementStartRecord(),
            BamlRecordType.OptimizedStaticResource => new OptimizedStaticResourceRecord(),
            BamlRecordType.PIMapping => new PIMappingRecord(),
            BamlRecordType.PresentationOptionsAttribute => new PresentationOptionsAttributeRecord(),
            BamlRecordType.Property => new PropertyRecord(),
            BamlRecordType.PropertyArrayEnd => new PropertyArrayEndRecord(),
            BamlRecordType.PropertyArrayStart => new PropertyArrayStartRecord(),
            BamlRecordType.PropertyComplexEnd => new PropertyComplexEndRecord(),
            BamlRecordType.PropertyComplexStart => new PropertyComplexStartRecord(),
            BamlRecordType.PropertyCustom => new PropertyCustomRecord(),
            BamlRecordType.PropertyDictionaryEnd => new PropertyDictionaryEndRecord(),
            BamlRecordType.PropertyDictionaryStart => new PropertyDictionaryStartRecord(),
            BamlRecordType.PropertyListEnd => new PropertyListEndRecord(),
            BamlRecordType.PropertyListStart => new PropertyListStartRecord(),
            BamlRecordType.PropertyStringReference => new PropertyStringReferenceRecord(),
            BamlRecordType.PropertyTypeReference => new PropertyTypeReferenceRecord(),
            BamlRecordType.PropertyWithConverter => new PropertyWithConverterRecord(),
            BamlRecordType.PropertyWithExtension => new PropertyWithExtensionRecord(),
            BamlRecordType.PropertyWithStaticResourceId => new PropertyWithStaticResourceIdRecord(),
            BamlRecordType.RoutedEvent => new RoutedEventRecord(),
            BamlRecordType.StaticResourceEnd => new StaticResourceEndRecord(),
            BamlRecordType.StaticResourceId => new StaticResourceIdRecord(),
            BamlRecordType.StaticResourceStart => new StaticResourceStartRecord(),
            BamlRecordType.StringInfo => new StringInfoRecord(),
            BamlRecordType.Text => new TextRecord(),
            BamlRecordType.TextWithConverter => new TextWithConverterRecord(),
            BamlRecordType.TextWithId => new TextWithIdRecord(),
            BamlRecordType.TypeInfo => new TypeInfoRecord(),
            BamlRecordType.TypeSerializerInfo => new TypeSerializerInfoRecord(),
            BamlRecordType.XmlnsProperty => new XmlnsPropertyRecord(),
            _ => throw new NotSupportedException("Unsupported BAML record: " + type)
        };
    }
}

internal static class BamlWriter
{
    public static void WriteDocument(BamlDocument document, Stream stream)
    {
        var writer = new BamlBinaryWriter(stream);
        {
            var unicode = new BinaryWriter(stream, Encoding.Unicode);
            var length = document.Signature.Length * 2;
            unicode.Write(length);
            unicode.Write(document.Signature.ToCharArray());
            unicode.Write(new byte[((length + 3) & ~3) - length]);
        }
        writer.Write(document.ReaderVersion.Major);
        writer.Write(document.ReaderVersion.Minor);
        writer.Write(document.UpdaterVersion.Major);
        writer.Write(document.UpdaterVersion.Minor);
        writer.Write(document.WriterVersion.Major);
        writer.Write(document.WriterVersion.Minor);

        var defers = new List<int>();
        for (var i = 0; i < document.Count; i++)
        {
            var record = document[i];
            record.Position = stream.Position;
            writer.Write((byte)record.Type);
            record.Write(writer);
            if (record is IBamlDeferRecord)
            {
                defers.Add(i);
            }
        }
        foreach (var i in defers)
        {
            ((IBamlDeferRecord)document[i]).WriteDefer(document, i, writer);
        }
    }
}
