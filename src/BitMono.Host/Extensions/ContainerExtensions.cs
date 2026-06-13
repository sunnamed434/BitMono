using BitMono.API.Protections;
using BitMono.Shared.DependencyInjection;
using BitMono.Shared.Logging;
using BitMono.Shared.Models;
using BitMono.Shared.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BitMono.Host.Extensions;

/// <summary>
/// Extension methods for configuring the BitMono container.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public static class BitMonoContainerExtensions
{
    private const string ProtectionsFileName = "BitMono.Protections.dll";
    private const string UnityFileName = "BitMono.Unity.dll";
    private const string DefaultPluginsDirectoryName = "plugins";

    /// <summary>
    /// Loads and registers protection assemblies, including any drop-in plugins (see #227).
    /// </summary>
    /// <param name="container">Container to register protections in</param>
    /// <param name="file">Optional path to protections DLL</param>
    /// <returns>Container for chaining</returns>
    public static Container AddProtections(this Container container, string? file = null)
    {
        var protectionsFilePath = file ?? Path.Combine(AppContext.BaseDirectory, ProtectionsFileName);
        var rawData = File.ReadAllBytes(protectionsFilePath);
        Assembly.Load(rawData);

        var unityFilePath = Path.Combine(AppContext.BaseDirectory, UnityFileName);
        if (File.Exists(unityFilePath))
        {
            var unityRawData = File.ReadAllBytes(unityFilePath);
            Assembly.Load(unityRawData);
        }

        var logger = container.GetService(typeof(ILogger)) as ILogger;

        // Load drop-in plugins before scanning so their protections are discovered alongside the built-ins.
        LoadPlugins(container, logger);

        var scanLogger = logger?.ForContext(typeof(BitMonoContainerExtensions));
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Collect all protection types (excluding IPhaseProtection)
        var protectionTypes = new List<Type>();
        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
                scanLogger?.Warning("Some types in '{0}' could not be loaded: {1}",
                    assembly.GetName().Name ?? assembly.FullName ?? "?",
                    string.Join("; ", ex.LoaderExceptions.Where(e => e != null).Select(e => e!.Message).Distinct()));
            }

            foreach (var type in types)
            {
                // Open generic definitions can't be instantiated by the container, so skip them.
                if (!type.IsClass || type.IsAbstract || !type.IsPublic || type.IsGenericTypeDefinition)
                    continue;

                // IPhaseProtection extends IProtection, so it MUST be checked first - phase protections
                // are driven by their pipeline, not registered as standalone protections.
                if (typeof(IPhaseProtection).IsAssignableFrom(type))
                    continue;

                if (typeof(IProtection).IsAssignableFrom(type))
                {
                    protectionTypes.Add(type);
                    // Register the concrete type
                    container.Register(type, type).AsSingleton();
                }
                else if (type.GetInterfaces().Any(i => i.Name == nameof(IProtection)))
                {
                    // Implements an interface *named* IProtection but not BitMono's contract - almost always
                    // a plugin shipping its own copy of BitMono.API, which would be silently ignored (#227).
                    scanLogger?.Warning(
                        "Type '{0}' in '{1}' looks like a protection but references a different BitMono.API and " +
                        "will be ignored. Reference BitMono.API with Private=\"false\" and don't ship it beside your plugin.",
                        type.FullName ?? type.Name, type.Assembly.GetName().Name ?? "?");
                }
            }
        }

        // Register the collection factory
        container.Register<ICollection<IProtection>>(() =>
        {
            var list = new List<IProtection>();
            foreach (var protType in protectionTypes)
            {
                try
                {
                    var instance = container.GetService(protType);
                    if (instance is IProtection protection)
                    {
                        list.Add(protection);
                    }
                }
                catch (Exception ex)
                {
                    // A broken plugin protection must not abort the whole run - log and skip it (#227).
                    scanLogger?.Error(ex,
                        "Failed to instantiate protection '{0}'. Its constructor parameters must be resolvable by the container.",
                        protType.FullName ?? protType.Name);
                }
            }
            return list;
        }).AsSingleton();

        return container;
    }

    private static void LoadPlugins(Container container, ILogger? logger)
    {
        // No logger means the host wasn't fully set up; plugin loading relies on logging for diagnostics.
        if (logger == null)
            return;

        var obfuscationSettings = container.GetService(typeof(ObfuscationSettings)) as ObfuscationSettings;
        var directoryName = obfuscationSettings?.PluginsDirectoryName;
        if (string.IsNullOrWhiteSpace(directoryName))
            directoryName = DefaultPluginsDirectoryName;

        var pluginsDirectory = Path.IsPathRooted(directoryName)
            ? directoryName!
            : Path.Combine(AppContext.BaseDirectory, directoryName!);

        new PluginLoader(pluginsDirectory, logger).LoadPlugins();
    }
}
