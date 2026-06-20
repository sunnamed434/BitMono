namespace BitMono.IL2CPP.Tests;

// Builds minimal-but-valid global-metadata.dat bytes for tests: a 32-byte header prefix followed by the
// identifier-name region, the literal-data region, and the literal table. Mirrors the on-disk layout
// (little-endian, NUL-terminated names, 8-byte {length, dataIndex} literal entries).
internal static class SyntheticMetadata
{
    public static byte[] Build(int version, string[] names, string[] literals)
    {
        using var nameRegion = new MemoryStream();
        foreach (var name in names)
        {
            var encoded = Encoding.UTF8.GetBytes(name);
            nameRegion.Write(encoded, 0, encoded.Length);
            nameRegion.WriteByte(0);
        }
        var stringBytes = nameRegion.ToArray();

        using var literalData = new MemoryStream();
        using var literalTable = new MemoryStream();
        var tableWriter = new BinaryWriter(literalTable);
        foreach (var literal in literals)
        {
            var encoded = Encoding.UTF8.GetBytes(literal);
            tableWriter.Write(encoded.Length);            // length
            tableWriter.Write((int)literalData.Length);   // dataIndex
            literalData.Write(encoded, 0, encoded.Length);
        }
        var litData = literalData.ToArray();
        var litTable = literalTable.ToArray();

        const int headerSize = 32;
        var stringOffset = headerSize;
        var litDataOffset = stringOffset + stringBytes.Length;
        var litTableOffset = litDataOffset + litData.Length;

        using var output = new MemoryStream();
        var writer = new BinaryWriter(output);
        writer.Write(GlobalMetadataFile.Magic); // 0xFAB11BAF
        writer.Write(version);
        writer.Write(litTableOffset);           // stringLiteralOffset
        writer.Write(litTable.Length);          // stringLiteralSize
        writer.Write(litDataOffset);            // stringLiteralDataOffset
        writer.Write(litData.Length);           // stringLiteralDataSize
        writer.Write(stringOffset);             // stringOffset
        writer.Write(stringBytes.Length);       // stringSize
        writer.Write(stringBytes);
        writer.Write(litData);
        writer.Write(litTable);
        return output.ToArray();
    }

    // nameIndex (offset relative to the string region) of a name, in the order Build packs them.
    public static int IndexOfName(string[] names, string target)
    {
        var index = 0;
        foreach (var name in names)
        {
            if (name == target)
            {
                return index;
            }
            index += Encoding.UTF8.GetByteCount(name) + 1; // +1 for the NUL terminator
        }
        throw new ArgumentException($"'{target}' is not in the names array.");
    }

    public static void WriteInt32(byte[] data, int offset, int value)
    {
        data[offset] = (byte)value;
        data[offset + 1] = (byte)(value >> 8);
        data[offset + 2] = (byte)(value >> 16);
        data[offset + 3] = (byte)(value >> 24);
    }
}
