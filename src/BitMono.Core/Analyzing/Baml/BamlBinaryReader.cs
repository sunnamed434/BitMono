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
