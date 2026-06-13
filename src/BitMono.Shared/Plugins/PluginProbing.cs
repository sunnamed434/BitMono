namespace BitMono.Shared.Plugins;

/// <summary>
/// Pure file-system helpers for locating plugin assemblies and their probe directories.
/// Kept side-effect free (no assembly loading) so the discovery logic can be unit tested. See #227.
/// </summary>
public static class PluginProbing
{
    /// <summary>
    /// Enumerates candidate plugin assemblies in <paramref name="pluginsDirectory"/>. A plugin lives either
    /// directly in the plugins root (flat layout, <c>Plugins/MyPlugin.dll</c>) or one level down in its own
    /// folder (<c>Plugins/MyPlugin/MyPlugin.dll</c>). Anything deeper (e.g. a <c>Plugins/MyPlugin/libs</c>
    /// folder of NuGet dependencies) is intentionally not treated as a plugin - those are resolved on
    /// demand by the <see cref="PluginLoader"/>'s assembly-resolve handler.
    /// </summary>
    public static IReadOnlyList<string> EnumeratePluginAssemblies(string pluginsDirectory)
    {
        if (string.IsNullOrWhiteSpace(pluginsDirectory) || !Directory.Exists(pluginsDirectory))
        {
            return [];
        }

        var result = new List<string>();
        result.AddRange(Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly));
        foreach (var subDirectory in Directory.GetDirectories(pluginsDirectory))
        {
            result.AddRange(Directory.GetFiles(subDirectory, "*.dll", SearchOption.TopDirectoryOnly));
        }
        return result;
    }

    /// <summary>
    /// Returns the distinct directories probed when resolving a plugin's dependencies: the plugins root
    /// and every directory beneath it (recursively), so dependencies may be placed in nested folders.
    /// </summary>
    public static IReadOnlyList<string> GetProbeDirectories(string pluginsDirectory)
    {
        if (string.IsNullOrWhiteSpace(pluginsDirectory) || !Directory.Exists(pluginsDirectory))
        {
            return [];
        }

        var directories = new List<string> { pluginsDirectory };
        directories.AddRange(Directory.GetDirectories(pluginsDirectory, "*", SearchOption.AllDirectories));
        return directories
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
