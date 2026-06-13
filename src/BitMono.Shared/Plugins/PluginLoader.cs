using System.Collections.Concurrent;
using System.Reflection;
using BitMono.Shared.Logging;

namespace BitMono.Shared.Plugins;

/// <summary>
/// Loads user-supplied plugin assemblies (custom protections) from a directory and resolves their
/// external dependencies through a single <see cref="AppDomain.AssemblyResolve"/> handler. This is the
/// only mechanism that works across every BitMono target (net462, netstandard2.0/2.1, net6-net10) -
/// <c>AssemblyLoadContext</c> does not exist on the older targets. See #227.
/// </summary>
public sealed class PluginLoader
{
    // The assembly-resolve handler is AppDomain-global, so its state is shared across every loader and
    // hooked exactly once. Repeated LoadPlugins() calls simply contribute more probe directories, which
    // avoids stacking duplicate handlers if BitMono is used as a long-lived engine rather than the CLI.
    private static readonly object SyncRoot = new();
    private static readonly HashSet<string> ProbeDirectories = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, Assembly> ResolvedDependencies = new(StringComparer.OrdinalIgnoreCase);
    private static ILogger? _resolverLogger;
    private static bool _resolverHooked;

    private readonly string _pluginsDirectory;
    private readonly ILogger _logger;

    public PluginLoader(string pluginsDirectory, ILogger logger)
    {
        _pluginsDirectory = pluginsDirectory;
        _logger = logger.ForContext<PluginLoader>();
    }

    /// <summary>
    /// Loads every plugin assembly found in the plugins directory. Failures are logged and skipped so a
    /// single broken plugin never aborts obfuscation. Returns the assemblies that loaded successfully.
    /// </summary>
    public IReadOnlyList<Assembly> LoadPlugins()
    {
        var loaded = new List<Assembly>();
        if (!Directory.Exists(_pluginsDirectory))
        {
            _logger.Debug("Plugins directory '{0}' not found, skipping plugin loading.", _pluginsDirectory);
            return loaded;
        }

        var assemblyFiles = PluginProbing.EnumeratePluginAssemblies(_pluginsDirectory);
        if (assemblyFiles.Count == 0)
        {
            _logger.Debug("No plugin assemblies found in '{0}'.", _pluginsDirectory);
            return loaded;
        }

        // Register probe directories and hook the resolver BEFORE loading anything so a plugin's
        // dependencies can be found the moment its types are touched.
        RegisterProbeDirectories(PluginProbing.GetProbeDirectories(_pluginsDirectory));

        foreach (var file in assemblyFiles)
        {
            try
            {
                // LoadFrom (not Load(byte[])) keeps the file path, so sibling and transitive
                // dependencies probe correctly and the AssemblyResolve fallback has somewhere to look.
                var assembly = Assembly.LoadFrom(file);
                loaded.Add(assembly);
                _logger.Information("Loaded plugin: {0} v{1}", assembly.GetName().Name, assembly.GetName().Version);
            }
            catch (BadImageFormatException)
            {
                // A native/non-.NET dll dropped next to a plugin - skip quietly, it is not an assembly.
                _logger.Debug("Skipped non-managed file '{0}'.", file);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load plugin '{0}'.", file);
            }
        }

        _logger.Information("({0}) plugin assembly(ies) loaded from '{1}'.", loaded.Count, _pluginsDirectory);
        return loaded;
    }

    private void RegisterProbeDirectories(IEnumerable<string> directories)
    {
        lock (SyncRoot)
        {
            foreach (var directory in directories)
            {
                ProbeDirectories.Add(directory);
            }
            if (!_resolverHooked)
            {
                _resolverLogger = _logger;
                AppDomain.CurrentDomain.AssemblyResolve += ResolvePluginDependency;
                _resolverHooked = true;
            }
        }
    }

    private static Assembly? ResolvePluginDependency(object? sender, ResolveEventArgs args)
    {
        var simpleName = new AssemblyName(args.Name).Name;
        // Satellite/resource assemblies are never plugin dependencies - let the CLR fall back.
        if (string.IsNullOrEmpty(simpleName) || simpleName!.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (ResolvedDependencies.TryGetValue(simpleName, out var cached))
        {
            return cached;
        }

        // 1. Prefer an assembly already loaded in the host (BitMono.API, AsmResolver, ...) so plugins bind
        //    to the host's contract types instead of a shadow copy that may sit beside the plugin.
        var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(x => !x.IsDynamic &&
                string.Equals(x.GetName().Name, simpleName, StringComparison.OrdinalIgnoreCase));
        if (alreadyLoaded != null)
        {
            ResolvedDependencies[simpleName] = alreadyLoaded;
            return alreadyLoaded;
        }

        // 2. Probe the plugin directories. Only LoadFrom is safe here - calling Assembly.Load would
        //    re-raise AssemblyResolve and StackOverflow.
        string[] directories;
        lock (SyncRoot)
        {
            directories = new string[ProbeDirectories.Count];
            ProbeDirectories.CopyTo(directories);
        }
        foreach (var directory in directories)
        {
            var candidate = Path.Combine(directory, simpleName + ".dll");
            if (!File.Exists(candidate))
            {
                continue;
            }
            try
            {
                var dependency = Assembly.LoadFrom(candidate);
                ResolvedDependencies[simpleName] = dependency;
                _resolverLogger?.Debug("Resolved plugin dependency '{0}' from '{1}'.", simpleName, candidate);
                return dependency;
            }
            catch (Exception ex)
            {
                _resolverLogger?.Warning("Failed to load plugin dependency '{0}': {1}", candidate, ex.Message);
            }
        }
        // Not found: deliberately do NOT cache the miss, so a dependency that becomes available later can
        // still resolve, and the CLR's own fallbacks are not short-circuited on every failed bind.
        return null;
    }
}
