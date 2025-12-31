using BitMono.API.Protections;
using BitMono.Shared.DependencyInjection;
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

    /// <summary>
    /// Loads and registers protection assemblies.
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
            }

            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract || !type.IsPublic)
                    continue;

                // Skip IPhaseProtection, only register IProtection
                if (type.GetInterface(nameof(IPhaseProtection)) != null)
                    continue;

                if (type.GetInterface(nameof(IProtection)) != null)
                {
                    protectionTypes.Add(type);
                    // Register the concrete type
                    container.Register(type, type).AsSingleton();
                }
            }
        }

        // Register the collection factory
        container.Register<ICollection<IProtection>>(() =>
        {
            var list = new List<IProtection>();
            foreach (var protType in protectionTypes)
            {
                var instance = container.GetService(protType);
                if (instance is IProtection protection)
                {
                    list.Add(protection);
                }
            }
            return list;
        }).AsSingleton();

        return container;
    }
}
