using System.IO;

namespace BitMono.Core.Analyzing.Baml;

// Full read+write model of WPF BAML records, used by the BAML rewriter so a modified document can be
// serialized back byte-for-byte. The format is Microsoft's WPF BAML; see also ConfuserEx for a
// reference implementation. Round-trip (read -> write) must be identical on untouched BAML.

internal abstract class BamlRecord
{
    public abstract BamlRecordType Type { get; }
    public long Position { get; set; }
    public abstract void Read(BamlBinaryReader reader);
    public abstract void Write(BamlBinaryWriter writer);
}

internal abstract class SizedBamlRecord : BamlRecord
{
    public override void Read(BamlBinaryReader reader)
    {
        var start = reader.BaseStream.Position;
        var size = reader.ReadEncodedInt();
        ReadData(reader, size - (int)(reader.BaseStream.Position - start));
    }

    public override void Write(BamlBinaryWriter writer)
    {
        var start = writer.BaseStream.Position;
        WriteData(writer);
        var size = (int)(writer.BaseStream.Position - start);
        size = BamlBinaryWriter.SizeofEncodedInt(BamlBinaryWriter.SizeofEncodedInt(size) + size) + size;
        writer.BaseStream.Position = start;
        writer.WriteEncodedInt(size);
        WriteData(writer);
    }

    protected abstract void ReadData(BamlBinaryReader reader, int size);
    protected abstract void WriteData(BamlBinaryWriter writer);
}

internal interface IBamlDeferRecord
{
    BamlRecord Record { get; set; }
    void ReadDefer(BamlDocument doc, int index, Func<long, BamlRecord> resolve);
    void WriteDefer(BamlDocument doc, int index, BinaryWriter writer);
}

internal sealed class XmlnsPropertyRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.XmlnsProperty;
    public string Prefix { get; set; }
    public string XmlNamespace { get; set; }
    public ushort[] AssemblyIds { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        Prefix = reader.ReadString();
        XmlNamespace = reader.ReadString();
        AssemblyIds = new ushort[reader.ReadUInt16()];
        for (var i = 0; i < AssemblyIds.Length; i++)
            AssemblyIds[i] = reader.ReadUInt16();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(Prefix);
        writer.Write(XmlNamespace);
        writer.Write((ushort)AssemblyIds.Length);
        foreach (var id in AssemblyIds)
            writer.Write(id);
    }
}

internal sealed class PresentationOptionsAttributeRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.PresentationOptionsAttribute;
    public string Value { get; set; }
    public ushort NameId { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        Value = reader.ReadString();
        NameId = reader.ReadUInt16();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(Value);
        writer.Write(NameId);
    }
}

internal sealed class PIMappingRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.PIMapping;
    public string XmlNamespace { get; set; }
    public string ClrNamespace { get; set; }
    public ushort AssemblyId { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        XmlNamespace = reader.ReadString();
        ClrNamespace = reader.ReadString();
        AssemblyId = reader.ReadUInt16();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(XmlNamespace);
        writer.Write(ClrNamespace);
        writer.Write(AssemblyId);
    }
}

internal sealed class AssemblyInfoRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.AssemblyInfo;
    public ushort AssemblyId { get; set; }
    public string AssemblyFullName { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        AssemblyId = reader.ReadUInt16();
        AssemblyFullName = reader.ReadString();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(AssemblyId);
        writer.Write(AssemblyFullName);
    }
}

internal class PropertyRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.Property;
    public ushort AttributeId { get; set; }
    public string Value { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        AttributeId = reader.ReadUInt16();
        Value = reader.ReadString();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(AttributeId);
        writer.Write(Value);
    }
}

internal sealed class PropertyWithConverterRecord : PropertyRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyWithConverter;
    public ushort ConverterTypeId { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        base.ReadData(reader, size);
        ConverterTypeId = reader.ReadUInt16();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        base.WriteData(writer);
        writer.Write(ConverterTypeId);
    }
}

internal sealed class PropertyCustomRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyCustom;
    public ushort AttributeId { get; set; }
    public ushort SerializerTypeId { get; set; }
    public byte[] Data { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        var start = reader.BaseStream.Position;
        AttributeId = reader.ReadUInt16();
        SerializerTypeId = reader.ReadUInt16();
        Data = reader.ReadBytes(size - (int)(reader.BaseStream.Position - start));
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(AttributeId);
        writer.Write(SerializerTypeId);
        writer.Write(Data);
    }
}

internal sealed class DefAttributeRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.DefAttribute;
    public string Value { get; set; }
    public ushort NameId { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        Value = reader.ReadString();
        NameId = reader.ReadUInt16();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(Value);
        writer.Write(NameId);
    }
}

internal sealed class DefAttributeKeyStringRecord : SizedBamlRecord, IBamlDeferRecord
{
    internal uint pos = 0xffffffff;
    public override BamlRecordType Type => BamlRecordType.DefAttributeKeyString;
    public ushort ValueId { get; set; }
    public bool Shared { get; set; }
    public bool SharedSet { get; set; }
    public BamlRecord Record { get; set; }

    public void ReadDefer(BamlDocument doc, int index, Func<long, BamlRecord> resolve)
    {
        BamlDeferHelper.NavigateKeys(doc, ref index);
        Record = resolve(doc[index].Position + pos);
    }

    public void WriteDefer(BamlDocument doc, int index, BinaryWriter writer)
    {
        BamlDeferHelper.NavigateKeys(doc, ref index);
        writer.BaseStream.Seek(pos, SeekOrigin.Begin);
        writer.Write((uint)(Record.Position - doc[index].Position));
    }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        ValueId = reader.ReadUInt16();
        pos = reader.ReadUInt32();
        Shared = reader.ReadBoolean();
        SharedSet = reader.ReadBoolean();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(ValueId);
        pos = (uint)writer.BaseStream.Position;
        writer.Write((uint)0);
        writer.Write(Shared);
        writer.Write(SharedSet);
    }
}

internal class TypeInfoRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.TypeInfo;
    public ushort TypeId { get; set; }
    public ushort AssemblyId { get; set; }
    public string TypeFullName { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        TypeId = reader.ReadUInt16();
        AssemblyId = reader.ReadUInt16();
        TypeFullName = reader.ReadString();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(TypeId);
        writer.Write(AssemblyId);
        writer.Write(TypeFullName);
    }
}

internal sealed class TypeSerializerInfoRecord : TypeInfoRecord
{
    public override BamlRecordType Type => BamlRecordType.TypeSerializerInfo;
    public ushort SerializerTypeId { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        base.ReadData(reader, size);
        SerializerTypeId = reader.ReadUInt16();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        base.WriteData(writer);
        writer.Write(SerializerTypeId);
    }
}

internal sealed class AttributeInfoRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.AttributeInfo;
    public ushort AttributeId { get; set; }
    public ushort OwnerTypeId { get; set; }
    public byte AttributeUsage { get; set; }
    public string Name { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        AttributeId = reader.ReadUInt16();
        OwnerTypeId = reader.ReadUInt16();
        AttributeUsage = reader.ReadByte();
        Name = reader.ReadString();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(AttributeId);
        writer.Write(OwnerTypeId);
        writer.Write(AttributeUsage);
        writer.Write(Name);
    }
}

internal sealed class StringInfoRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.StringInfo;
    public ushort StringId { get; set; }
    public string Value { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        StringId = reader.ReadUInt16();
        Value = reader.ReadString();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(StringId);
        writer.Write(Value);
    }
}

internal class TextRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.Text;
    public string Value { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        Value = reader.ReadString();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(Value);
    }
}

internal sealed class TextWithConverterRecord : TextRecord
{
    public override BamlRecordType Type => BamlRecordType.TextWithConverter;
    public ushort ConverterTypeId { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        base.ReadData(reader, size);
        ConverterTypeId = reader.ReadUInt16();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        base.WriteData(writer);
        writer.Write(ConverterTypeId);
    }
}

internal sealed class TextWithIdRecord : TextRecord
{
    public override BamlRecordType Type => BamlRecordType.TextWithId;
    public ushort ValueId { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        ValueId = reader.ReadUInt16();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(ValueId);
    }
}

internal sealed class LiteralContentRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.LiteralContent;
    public string Value { get; set; }
    public uint Reserved0 { get; set; }
    public uint Reserved1 { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        Value = reader.ReadString();
        Reserved0 = reader.ReadUInt32();
        Reserved1 = reader.ReadUInt32();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(Value);
        writer.Write(Reserved0);
        writer.Write(Reserved1);
    }
}

internal sealed class RoutedEventRecord : SizedBamlRecord
{
    public override BamlRecordType Type => BamlRecordType.RoutedEvent;
    public string Value { get; set; }
    public ushort AttributeId { get; set; }

    protected override void ReadData(BamlBinaryReader reader, int size)
    {
        AttributeId = reader.ReadUInt16();
        Value = reader.ReadString();
    }

    protected override void WriteData(BamlBinaryWriter writer)
    {
        writer.Write(AttributeId);
        writer.Write(Value);
    }
}

internal sealed class DocumentStartRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.DocumentStart;
    public bool LoadAsync { get; set; }
    public uint MaxAsyncRecords { get; set; }
    public bool DebugBaml { get; set; }

    public override void Read(BamlBinaryReader reader)
    {
        LoadAsync = reader.ReadBoolean();
        MaxAsyncRecords = reader.ReadUInt32();
        DebugBaml = reader.ReadBoolean();
    }

    public override void Write(BamlBinaryWriter writer)
    {
        writer.Write(LoadAsync);
        writer.Write(MaxAsyncRecords);
        writer.Write(DebugBaml);
    }
}

internal sealed class DocumentEndRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.DocumentEnd;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal class ElementStartRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.ElementStart;
    public ushort TypeId { get; set; }
    public byte Flags { get; set; }

    public override void Read(BamlBinaryReader reader)
    {
        TypeId = reader.ReadUInt16();
        Flags = reader.ReadByte();
    }

    public override void Write(BamlBinaryWriter writer)
    {
        writer.Write(TypeId);
        writer.Write(Flags);
    }
}

internal sealed class ElementEndRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.ElementEnd;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal class DefAttributeKeyTypeRecord : ElementStartRecord, IBamlDeferRecord
{
    internal uint pos = 0xffffffff;
    public override BamlRecordType Type => BamlRecordType.DefAttributeKeyType;
    public bool Shared { get; set; }
    public bool SharedSet { get; set; }
    public BamlRecord Record { get; set; }

    public void ReadDefer(BamlDocument doc, int index, Func<long, BamlRecord> resolve)
    {
        BamlDeferHelper.NavigateKeys(doc, ref index);
        Record = resolve(doc[index].Position + pos);
    }

    public void WriteDefer(BamlDocument doc, int index, BinaryWriter writer)
    {
        BamlDeferHelper.NavigateKeys(doc, ref index);
        writer.BaseStream.Seek(pos, SeekOrigin.Begin);
        writer.Write((uint)(Record.Position - doc[index].Position));
    }

    public override void Read(BamlBinaryReader reader)
    {
        base.Read(reader);
        pos = reader.ReadUInt32();
        Shared = reader.ReadBoolean();
        SharedSet = reader.ReadBoolean();
    }

    public override void Write(BamlBinaryWriter writer)
    {
        base.Write(writer);
        pos = (uint)writer.BaseStream.Position;
        writer.Write((uint)0);
        writer.Write(Shared);
        writer.Write(SharedSet);
    }
}

internal sealed class KeyElementStartRecord : DefAttributeKeyTypeRecord
{
    public override BamlRecordType Type => BamlRecordType.KeyElementStart;
}

internal sealed class KeyElementEndRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.KeyElementEnd;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal sealed class ConnectionIdRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.ConnectionId;
    public uint ConnectionId { get; set; }

    public override void Read(BamlBinaryReader reader) => ConnectionId = reader.ReadUInt32();
    public override void Write(BamlBinaryWriter writer) => writer.Write(ConnectionId);
}

internal sealed class PropertyWithExtensionRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyWithExtension;
    public ushort AttributeId { get; set; }
    public ushort Flags { get; set; }
    public ushort ValueId { get; set; }

    public override void Read(BamlBinaryReader reader)
    {
        AttributeId = reader.ReadUInt16();
        Flags = reader.ReadUInt16();
        ValueId = reader.ReadUInt16();
    }

    public override void Write(BamlBinaryWriter writer)
    {
        writer.Write(AttributeId);
        writer.Write(Flags);
        writer.Write(ValueId);
    }
}

internal class PropertyComplexStartRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyComplexStart;
    public ushort AttributeId { get; set; }

    public override void Read(BamlBinaryReader reader) => AttributeId = reader.ReadUInt16();
    public override void Write(BamlBinaryWriter writer) => writer.Write(AttributeId);
}

internal sealed class PropertyComplexEndRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyComplexEnd;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal sealed class PropertyTypeReferenceRecord : PropertyComplexStartRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyTypeReference;
    public ushort TypeId { get; set; }

    public override void Read(BamlBinaryReader reader)
    {
        base.Read(reader);
        TypeId = reader.ReadUInt16();
    }

    public override void Write(BamlBinaryWriter writer)
    {
        base.Write(writer);
        writer.Write(TypeId);
    }
}

internal sealed class PropertyStringReferenceRecord : PropertyComplexStartRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyStringReference;
    public ushort StringId { get; set; }

    public override void Read(BamlBinaryReader reader)
    {
        base.Read(reader);
        StringId = reader.ReadUInt16();
    }

    public override void Write(BamlBinaryWriter writer)
    {
        base.Write(writer);
        writer.Write(StringId);
    }
}

internal sealed class PropertyListStartRecord : PropertyComplexStartRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyListStart;
}

internal sealed class PropertyListEndRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyListEnd;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal sealed class PropertyDictionaryStartRecord : PropertyComplexStartRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyDictionaryStart;
}

internal sealed class PropertyDictionaryEndRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyDictionaryEnd;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal sealed class PropertyArrayStartRecord : PropertyComplexStartRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyArrayStart;
}

internal sealed class PropertyArrayEndRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyArrayEnd;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal sealed class ConstructorParametersStartRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.ConstructorParametersStart;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal sealed class ConstructorParametersEndRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.ConstructorParametersEnd;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal sealed class ConstructorParameterTypeRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.ConstructorParameterType;
    public ushort TypeId { get; set; }

    public override void Read(BamlBinaryReader reader) => TypeId = reader.ReadUInt16();
    public override void Write(BamlBinaryWriter writer) => writer.Write(TypeId);
}

internal sealed class ContentPropertyRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.ContentProperty;
    public ushort AttributeId { get; set; }

    public override void Read(BamlBinaryReader reader) => AttributeId = reader.ReadUInt16();
    public override void Write(BamlBinaryWriter writer) => writer.Write(AttributeId);
}

internal sealed class DeferableContentStartRecord : BamlRecord, IBamlDeferRecord
{
    long pos;
    internal uint size = 0xffffffff;
    public override BamlRecordType Type => BamlRecordType.DeferableContentStart;
    public BamlRecord Record { get; set; }

    public void ReadDefer(BamlDocument doc, int index, Func<long, BamlRecord> resolve)
    {
        Record = resolve(pos + size);
    }

    public void WriteDefer(BamlDocument doc, int index, BinaryWriter writer)
    {
        writer.BaseStream.Seek(pos, SeekOrigin.Begin);
        writer.Write((uint)(Record.Position - (pos + 4)));
    }

    public override void Read(BamlBinaryReader reader)
    {
        size = reader.ReadUInt32();
        pos = reader.BaseStream.Position;
    }

    public override void Write(BamlBinaryWriter writer)
    {
        pos = writer.BaseStream.Position;
        writer.Write((uint)0);
    }
}

internal sealed class StaticResourceStartRecord : ElementStartRecord
{
    public override BamlRecordType Type => BamlRecordType.StaticResourceStart;
}

internal sealed class StaticResourceEndRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.StaticResourceEnd;
    public override void Read(BamlBinaryReader reader) { }
    public override void Write(BamlBinaryWriter writer) { }
}

internal class StaticResourceIdRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.StaticResourceId;
    public ushort StaticResourceId { get; set; }

    public override void Read(BamlBinaryReader reader) => StaticResourceId = reader.ReadUInt16();
    public override void Write(BamlBinaryWriter writer) => writer.Write(StaticResourceId);
}

internal sealed class PropertyWithStaticResourceIdRecord : StaticResourceIdRecord
{
    public override BamlRecordType Type => BamlRecordType.PropertyWithStaticResourceId;
    public ushort AttributeId { get; set; }

    public override void Read(BamlBinaryReader reader)
    {
        AttributeId = reader.ReadUInt16();
        base.Read(reader);
    }

    public override void Write(BamlBinaryWriter writer)
    {
        writer.Write(AttributeId);
        base.Write(writer);
    }
}

internal sealed class OptimizedStaticResourceRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.OptimizedStaticResource;
    public byte Flags { get; set; }
    public ushort ValueId { get; set; }

    public override void Read(BamlBinaryReader reader)
    {
        Flags = reader.ReadByte();
        ValueId = reader.ReadUInt16();
    }

    public override void Write(BamlBinaryWriter writer)
    {
        writer.Write(Flags);
        writer.Write(ValueId);
    }
}

internal sealed class LineNumberAndPositionRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.LineNumberAndPosition;
    public uint LineNumber { get; set; }
    public uint LinePosition { get; set; }

    public override void Read(BamlBinaryReader reader)
    {
        LineNumber = reader.ReadUInt32();
        LinePosition = reader.ReadUInt32();
    }

    public override void Write(BamlBinaryWriter writer)
    {
        writer.Write(LineNumber);
        writer.Write(LinePosition);
    }
}

internal sealed class LinePositionRecord : BamlRecord
{
    public override BamlRecordType Type => BamlRecordType.LinePosition;
    public uint LinePosition { get; set; }

    public override void Read(BamlBinaryReader reader) => LinePosition = reader.ReadUInt32();
    public override void Write(BamlBinaryWriter writer) => writer.Write(LinePosition);
}

internal sealed class NamedElementStartRecord : ElementStartRecord
{
    public override BamlRecordType Type => BamlRecordType.NamedElementStart;
    public string RuntimeName { get; set; }

    public override void Read(BamlBinaryReader reader)
    {
        TypeId = reader.ReadUInt16();
        RuntimeName = reader.ReadString();
    }

    public override void Write(BamlBinaryWriter writer)
    {
        writer.Write(TypeId);
        if (RuntimeName != null)
            writer.Write(RuntimeName);
    }
}

internal static class BamlDeferHelper
{
    // Walks forward past consecutive key / static-resource records to the record a deferred key
    // anchors to (matching the WPF reader's behaviour).
    public static void NavigateKeys(BamlDocument doc, ref int index)
    {
        var keys = true;
        do
        {
            switch (doc[index].Type)
            {
                case BamlRecordType.DefAttributeKeyString:
                case BamlRecordType.DefAttributeKeyType:
                case BamlRecordType.OptimizedStaticResource:
                    keys = true;
                    break;
                case BamlRecordType.StaticResourceStart:
                    NavigateTree(doc, BamlRecordType.StaticResourceStart, BamlRecordType.StaticResourceEnd, ref index);
                    keys = true;
                    break;
                case BamlRecordType.KeyElementStart:
                    NavigateTree(doc, BamlRecordType.KeyElementStart, BamlRecordType.KeyElementEnd, ref index);
                    keys = true;
                    break;
                default:
                    keys = false;
                    index--;
                    break;
            }
            index++;
        } while (keys);
    }

    private static void NavigateTree(BamlDocument doc, BamlRecordType start, BamlRecordType end, ref int index)
    {
        index++;
        while (true)
        {
            if (doc[index].Type == start)
                NavigateTree(doc, start, end, ref index);
            else if (doc[index].Type == end)
                return;
            index++;
        }
    }
}
