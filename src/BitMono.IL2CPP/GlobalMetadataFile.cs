using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BitMono.IL2CPP;

/// <summary>
/// Read-only parser for Unity IL2CPP <c>global-metadata.dat</c>. Reads the header magic + version and
/// the two string regions Il2CppDumper reconstructs your code from: the identifier names (type/method/
/// field names) and the string literals from your code. This header prefix is identical across every
/// il2cpp metadata version, so no per-version branching is needed for it. The later per-version tables
/// (types, methods, fields, ...) are intentionally not parsed yet - that's the rest of #276.
/// </summary>
public sealed class GlobalMetadataFile
{
    /// <summary>The "sanity" magic every <c>global-metadata.dat</c> starts with.</summary>
    public const uint Magic = 0xFAB11BAF;

    // We only read the header prefix, which is identical across every il2cpp metadata version, so this range
    // is just a sanity guard - a version outside it means the bytes almost certainly aren't a
    // global-metadata.dat. Real versions run ~16 up to ~106 (Unity 6), hence the loose upper bound.
    private const int MinVersion = 16;
    private const int MaxVersion = 1000;

    // Header prefix: 8 little-endian int32 fields (magic, version, then 3 offset/size region pairs).
    private const int HeaderPrefixSize = 32;

    private GlobalMetadataFile(int version, IReadOnlyList<string> strings, IReadOnlyList<string> stringLiterals)
    {
        Version = version;
        Strings = strings;
        StringLiterals = stringLiterals;
    }

    /// <summary>The il2cpp metadata version (e.g. 24, 27, 29, 31).</summary>
    public int Version { get; }

    /// <summary>Identifier names (namespaces, type/method/field names) IL2CPP keeps for reflection etc.</summary>
    public IReadOnlyList<string> Strings { get; }

    /// <summary>String literals from your code (the <c>ldstr</c> values) - what string encryption targets.</summary>
    public IReadOnlyList<string> StringLiterals { get; }

    public static GlobalMetadataFile Read(string path) => Read(File.ReadAllBytes(path));

    public static GlobalMetadataFile Read(byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        var version = ValidateHeader(data);

        var stringLiteralOffset = ReadInt32(data, 8);
        var stringLiteralSize = ReadInt32(data, 12);
        var stringLiteralDataOffset = ReadInt32(data, 16);
        var stringLiteralDataSize = ReadInt32(data, 20);
        var stringOffset = ReadInt32(data, 24);
        var stringSize = ReadInt32(data, 28);

        var strings = ReadStrings(data, stringOffset, stringSize);
        var literals = ReadStringLiterals(
            data, stringLiteralOffset, stringLiteralSize, stringLiteralDataOffset, stringLiteralDataSize);
        return new GlobalMetadataFile(version, strings, literals);
    }

    // Identifier names are NUL-terminated UTF-8 packed back to back in [offset, offset + size).
    private static List<string> ReadStrings(byte[] data, int offset, int size)
    {
        CheckRegion(data, offset, size, "string");
        var result = new List<string>();
        var start = offset;
        var end = offset + size;
        for (var i = offset; i < end; i++)
        {
            if (data[i] != 0)
            {
                continue;
            }
            result.Add(Encoding.UTF8.GetString(data, start, i - start));
            start = i + 1;
        }
        if (start < end) // trailing bytes with no terminator
        {
            result.Add(Encoding.UTF8.GetString(data, start, end - start));
        }
        return result;
    }

    // Each entry is Il2CppStringLiteral { int length; int dataIndex } (8 bytes), pointing into the data region.
    private static List<string> ReadStringLiterals(
        byte[] data, int tableOffset, int tableSize, int dataOffset, int dataSize)
    {
        CheckRegion(data, tableOffset, tableSize, "stringLiteral");
        CheckRegion(data, dataOffset, dataSize, "stringLiteralData");
        var result = new List<string>();
        var count = tableSize / 8;
        for (var i = 0; i < count; i++)
        {
            var entry = tableOffset + i * 8;
            var length = ReadInt32(data, entry);
            var dataIndex = ReadInt32(data, entry + 4);
            if (length < 0 || dataIndex < 0 || (long)dataIndex + length > dataSize)
            {
                throw new InvalidDataException($"String literal #{i} is out of bounds.");
            }
            result.Add(Encoding.UTF8.GetString(data, dataOffset + dataIndex, length));
        }
        return result;
    }

    // Validates magic + version and returns the version. Shared with GlobalMetadataWriter so both reject the
    // same garbage. Assumes data is non-null (public entry points null-check first).
    internal static int ValidateHeader(byte[] data)
    {
        if (data.Length < HeaderPrefixSize)
        {
            throw new InvalidDataException("File is too small to be a global-metadata.dat.");
        }
        var magic = unchecked((uint)ReadInt32(data, 0));
        if (magic != Magic)
        {
            throw new InvalidDataException($"Not a global-metadata.dat: magic 0x{magic:X8} != 0x{Magic:X8}.");
        }
        var version = ReadInt32(data, 4);
        if (version < MinVersion || version > MaxVersion)
        {
            throw new InvalidDataException(
                $"Unsupported il2cpp metadata version {version} (expected {MinVersion}..{MaxVersion}).");
        }
        return version;
    }

    internal static void CheckRegion(byte[] data, int offset, int size, string name)
    {
        if (offset < 0 || size < 0 || (long)offset + size > data.Length)
        {
            throw new InvalidDataException(
                $"The {name} region (offset {offset}, size {size}) is outside the file.");
        }
    }

    // global-metadata.dat is always little-endian, regardless of host architecture.
    internal static int ReadInt32(byte[] data, int offset) =>
        data[offset] | data[offset + 1] << 8 | data[offset + 2] << 16 | data[offset + 3] << 24;
}
