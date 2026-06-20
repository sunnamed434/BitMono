#if NET6_0_OR_GREATER
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif

namespace BitMono.Shared;

/// <summary>
/// Cross-platform JSON helper. Uses the in-box System.Text.Json on net6.0+ and Newtonsoft.Json on
/// .NET Framework / netstandard, behind one API — so anything targeting a BitMono-supported runtime
/// (net462 … net10), including plugins, can read and write JSON without referencing a serializer of
/// its own. Tolerates // comments and trailing commas, and binds property names case-insensitively.
/// </summary>
public static class Json
{
#if NET6_0_OR_GREATER
    private static readonly JsonSerializerOptions Indented = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
    private static readonly JsonSerializerOptions Compact = new(Indented) { WriteIndented = false };
#endif

    /// <summary>Serializes a value to JSON text.</summary>
    public static string Serialize<T>(T value, bool indented = true)
    {
#if NET6_0_OR_GREATER
        return JsonSerializer.Serialize(value, indented ? Indented : Compact);
#else
        return JsonConvert.SerializeObject(value, indented ? Formatting.Indented : Formatting.None);
#endif
    }

    /// <summary>Deserializes JSON text to <typeparamref name="T"/> (<c>null</c> if the text is "null").</summary>
    public static T? Deserialize<T>(string json)
    {
#if NET6_0_OR_GREATER
        return JsonSerializer.Deserialize<T>(json, Indented);
#else
        return JsonConvert.DeserializeObject<T>(json);
#endif
    }

    /// <summary>Reads a JSON file into <typeparamref name="T"/>, or a new instance if the file doesn't exist.</summary>
    public static T LoadFile<T>(string path) where T : new()
    {
        if (!File.Exists(path))
            return new T();

        return Deserialize<T>(File.ReadAllText(path)) ?? new T();
    }

    /// <summary>Writes a value to a JSON file, creating the directory if needed.</summary>
    public static void SaveFile<T>(T value, string path, bool indented = true)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, Serialize(value, indented));
    }
}
