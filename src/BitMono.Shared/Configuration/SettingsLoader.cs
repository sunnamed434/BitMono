using Newtonsoft.Json;

namespace BitMono.Shared.Configuration;

/// <summary>
/// Simple JSON-based settings loader.
/// </summary>
public static class SettingsLoader
{
    /// <summary>
    /// Loads settings from a JSON file.
    /// </summary>
    /// <typeparam name="T">Settings type</typeparam>
    /// <param name="jsonPath">Path to the JSON file</param>
    /// <returns>Deserialized settings object, or new instance if file doesn't exist</returns>
    public static T Load<T>(string jsonPath) where T : new()
    {
        if (!File.Exists(jsonPath))
            return new T();

        var json = File.ReadAllText(jsonPath);
        return JsonConvert.DeserializeObject<T>(json) ?? new T();
    }

    /// <summary>
    /// Loads settings from a JSON file with variable substitution.
    /// Variables in format {{variableName}} will be replaced.
    /// </summary>
    /// <typeparam name="T">Settings type</typeparam>
    /// <param name="jsonPath">Path to the JSON file</param>
    /// <param name="variables">Dictionary of variable names and values to substitute</param>
    /// <returns>Deserialized settings object, or new instance if file doesn't exist</returns>
    public static T LoadWithVariables<T>(string jsonPath, Dictionary<string, string>? variables = null)
        where T : new()
    {
        if (!File.Exists(jsonPath))
            return new T();

        var json = File.ReadAllText(jsonPath);

        if (variables != null)
        {
            foreach (var kvp in variables)
            {
                json = json.Replace("{{" + kvp.Key + "}}", kvp.Value);
            }
        }

        return JsonConvert.DeserializeObject<T>(json) ?? new T();
    }

    /// <summary>
    /// Saves settings to a JSON file.
    /// </summary>
    /// <typeparam name="T">Settings type</typeparam>
    /// <param name="settings">Settings object to save</param>
    /// <param name="jsonPath">Path to save the JSON file</param>
    /// <param name="indented">Whether to format with indentation</param>
    public static void Save<T>(T settings, string jsonPath, bool indented = true)
    {
        var dir = Path.GetDirectoryName(jsonPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonConvert.SerializeObject(settings, indented ? Formatting.Indented : Formatting.None);
        File.WriteAllText(jsonPath, json);
    }
}
