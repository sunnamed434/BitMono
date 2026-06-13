using System.IO;

namespace BitMono.Core.Analyzing.Baml;

/// <summary>
/// <see cref="BinaryReader"/> for BAML streams. <see cref="BinaryReader.ReadString"/> already gives
/// the 7-bit-length-prefixed UTF-8 strings BAML uses; this only adds the 7-bit-encoded int used as a
/// record size prefix (reimplemented here because <c>Read7BitEncodedInt</c> is not public on every
/// target framework).
/// </summary>
internal sealed class BamlBinaryReader : BinaryReader
{
    public BamlBinaryReader(Stream stream) : base(stream)
    {
    }

    public int ReadEncodedInt()
    {
        var value = 0;
        var shift = 0;
        byte current;
        do
        {
            if (shift == 5 * 7)
            {
                throw new FormatException("Malformed 7-bit encoded int in BAML.");
            }
            current = ReadByte();
            value |= (current & 0x7F) << shift;
            shift += 7;
        } while ((current & 0x80) != 0);
        return value;
    }
}

/// <summary>
/// <see cref="BinaryWriter"/> counterpart of <see cref="BamlBinaryReader"/>, used when writing BAML
/// back out. <see cref="BinaryWriter.Write(string)"/> already emits the 7-bit-length-prefixed UTF-8
/// strings BAML uses; this adds the 7-bit-encoded int (record size prefix).
/// </summary>
internal sealed class BamlBinaryWriter : BinaryWriter
{
    public BamlBinaryWriter(Stream stream) : base(stream)
    {
    }

    public void WriteEncodedInt(int value)
    {
        var v = (uint)value;
        while (v >= 0x80)
        {
            Write((byte)(v | 0x80));
            v >>= 7;
        }
        Write((byte)v);
    }

    public static int SizeofEncodedInt(int value)
    {
        if ((value & ~0x7F) == 0) return 1;
        if ((value & ~0x3FFF) == 0) return 2;
        if ((value & ~0x1FFFFF) == 0) return 3;
        if ((value & ~0xFFFFFFF) == 0) return 4;
        return 5;
    }
}
