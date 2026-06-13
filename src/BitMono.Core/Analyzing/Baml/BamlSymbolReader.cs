using System.IO;
using System.Text;

namespace BitMono.Core.Analyzing.Baml;

/// <summary>
/// Minimal reader for compiled WPF XAML (BAML). BAML is Microsoft's binary form of XAML stored in
/// <c>&lt;assembly&gt;.g.resources</c>; this walks the records only far enough to discover which managed
/// types and members a XAML file references, so the renamer can avoid breaking them. It is read-only
/// (nothing here rewrites BAML). The BAML record layout is the WPF format; see also ConfuserEx for a
/// fuller implementation.
/// </summary>
internal sealed class BamlSymbolReader
{
    /// <summary>AssemblyId -&gt; assembly full name (as written in BAML).</summary>
    public Dictionary<ushort, string> Assemblies { get; } = new();

    /// <summary>TypeId -&gt; (AssemblyId, type full name).</summary>
    public Dictionary<ushort, BamlTypeRef> Types { get; } = new();

    /// <summary>Referenced members as (owner TypeId, member name).</summary>
    public List<BamlAttributeRef> Attributes { get; } = new();

    public readonly struct BamlTypeRef
    {
        public BamlTypeRef(ushort assemblyId, string fullName)
        {
            AssemblyId = assemblyId;
            FullName = fullName;
        }
        public ushort AssemblyId { get; }
        public string FullName { get; }
    }

    public readonly struct BamlAttributeRef
    {
        public BamlAttributeRef(ushort ownerTypeId, string name)
        {
            OwnerTypeId = ownerTypeId;
            Name = name;
        }
        public ushort OwnerTypeId { get; }
        public string Name { get; }
    }

    /// <summary>
    /// Parses a single BAML stream. Throws on a record shape it does not understand; callers should
    /// treat any exception as "could not analyze this document" and fall back to leaving symbols
    /// untouched.
    /// </summary>
    public static BamlSymbolReader Read(byte[] bamlStream)
    {
        var result = new BamlSymbolReader();
        using var stream = new MemoryStream(bamlStream, writable: false);
        result.ReadDocument(stream);
        return result;
    }

    private void ReadDocument(Stream stream)
    {
        var reader = new BamlBinaryReader(stream);

        // Header: [uint byteLength][UTF-16 signature][pad to 4-byte boundary][3 x version(ushort,ushort)].
        var signatureLength = reader.ReadUInt32();
        var signatureBytes = reader.ReadBytes((int)signatureLength);
        var signature = Encoding.Unicode.GetString(signatureBytes);
        reader.ReadBytes((int)(((signatureLength + 3) & ~3u) - signatureLength));
        if (signature != "MSBAML")
        {
            return;
        }
        reader.ReadBytes(12); // reader/updater/writer versions

        var length = stream.Length;
        while (stream.Position < length)
        {
            var type = (BamlRecordType)reader.ReadByte();
            ReadRecord(reader, stream, type);
        }
    }

    private void ReadRecord(BamlBinaryReader reader, Stream stream, BamlRecordType type)
    {
        if (IsSized(type))
        {
            var start = stream.Position;
            var size = reader.ReadEncodedInt();
            var next = start + size;
            switch (type)
            {
                case BamlRecordType.AssemblyInfo:
                {
                    var id = reader.ReadUInt16();
                    var name = reader.ReadString();
                    Assemblies[id] = name;
                    break;
                }
                case BamlRecordType.TypeInfo:
                case BamlRecordType.TypeSerializerInfo:
                {
                    var typeId = reader.ReadUInt16();
                    var assemblyId = reader.ReadUInt16();
                    var fullName = reader.ReadString();
                    Types[typeId] = new BamlTypeRef(assemblyId, fullName);
                    break;
                }
                case BamlRecordType.AttributeInfo:
                {
                    reader.ReadUInt16(); // attribute id
                    var ownerTypeId = reader.ReadUInt16();
                    reader.ReadByte(); // usage
                    var name = reader.ReadString();
                    Attributes.Add(new BamlAttributeRef(ownerTypeId, name));
                    break;
                }
            }
            // Re-sync to the next record regardless of what we read above. The size prefix makes
            // this robust even for the records we skip.
            stream.Position = next;
            return;
        }

        if (type == BamlRecordType.NamedElementStart)
        {
            reader.ReadUInt16();  // type id
            reader.ReadString();  // runtime name
            return;
        }

        var fixedSize = FixedSize(type);
        if (fixedSize < 0)
        {
            throw new NotSupportedException("Unsupported BAML record: " + type);
        }
        stream.Position += fixedSize;
    }

    // Records prefixed with a 7-bit-encoded size (SizedBamlRecord in the BAML format).
    private static bool IsSized(BamlRecordType type)
    {
        switch (type)
        {
            case BamlRecordType.XmlnsProperty:
            case BamlRecordType.PresentationOptionsAttribute:
            case BamlRecordType.PIMapping:
            case BamlRecordType.AssemblyInfo:
            case BamlRecordType.Property:
            case BamlRecordType.PropertyWithConverter:
            case BamlRecordType.PropertyCustom:
            case BamlRecordType.DefAttribute:
            case BamlRecordType.DefAttributeKeyString:
            case BamlRecordType.TypeInfo:
            case BamlRecordType.TypeSerializerInfo:
            case BamlRecordType.AttributeInfo:
            case BamlRecordType.StringInfo:
            case BamlRecordType.Text:
            case BamlRecordType.TextWithConverter:
            case BamlRecordType.TextWithId:
            case BamlRecordType.LiteralContent:
            case BamlRecordType.RoutedEvent:
                return true;
            default:
                return false;
        }
    }

    // Byte length of a fixed (non-size-prefixed) record's body, after the 1-byte record type.
    // -1 means "unknown" -> the document is treated as unparseable.
    private static int FixedSize(BamlRecordType type)
    {
        switch (type)
        {
            case BamlRecordType.DocumentEnd:
            case BamlRecordType.ElementEnd:
            case BamlRecordType.KeyElementEnd:
            case BamlRecordType.PropertyComplexEnd:
            case BamlRecordType.PropertyListEnd:
            case BamlRecordType.PropertyDictionaryEnd:
            case BamlRecordType.PropertyArrayEnd:
            case BamlRecordType.ConstructorParametersStart:
            case BamlRecordType.ConstructorParametersEnd:
            case BamlRecordType.StaticResourceEnd:
                return 0;
            case BamlRecordType.ContentProperty:
            case BamlRecordType.PropertyComplexStart:
            case BamlRecordType.PropertyListStart:
            case BamlRecordType.PropertyDictionaryStart:
            case BamlRecordType.PropertyArrayStart:
            case BamlRecordType.ConstructorParameterType:
            case BamlRecordType.StaticResourceId:
                return 2;
            case BamlRecordType.ElementStart:
            case BamlRecordType.StaticResourceStart:
            case BamlRecordType.OptimizedStaticResource:
                return 3;
            case BamlRecordType.ConnectionId:
            case BamlRecordType.PropertyTypeReference:
            case BamlRecordType.PropertyStringReference:
            case BamlRecordType.PropertyWithStaticResourceId:
            case BamlRecordType.DeferableContentStart:
            case BamlRecordType.LinePosition:
                return 4;
            case BamlRecordType.DocumentStart:
            case BamlRecordType.PropertyWithExtension:
                return 6;
            case BamlRecordType.LineNumberAndPosition:
                return 8;
            case BamlRecordType.KeyElementStart:
            case BamlRecordType.DefAttributeKeyType:
                return 9;
            default:
                return -1;
        }
    }
}
