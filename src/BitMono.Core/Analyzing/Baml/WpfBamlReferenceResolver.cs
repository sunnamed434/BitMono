using System.IO;
#if !NETSTANDARD
using System.Resources;
#endif

namespace BitMono.Core.Analyzing.Baml;

/// <summary>
/// Finds the managed types a WPF assembly references from its compiled XAML (BAML in
/// <c>&lt;assembly&gt;.g.resources</c>). The renamer excludes these so renaming doesn't leave the BAML
/// pointing at names that no longer exist (which crashes the app at XAML load with
/// <c>XamlParseException</c>). This is the safe baseline: it never rewrites the BAML, it only keeps
/// referenced symbols intact.
/// </summary>
public static class WpfBamlReferenceResolver
{
    /// <summary>
    /// Returns the in-module types referenced by the assembly's BAML. Returns an empty set on any
    /// problem (no WPF, unreadable/unsupported BAML, unsupported target framework) so callers can
    /// rely on it never throwing.
    /// </summary>
    public static HashSet<TypeDefinition> ResolveReferencedTypes(ModuleDefinition module)
    {
        var excluded = new HashSet<TypeDefinition>();
#if !NETSTANDARD
        var assemblyName = module.Assembly?.Name?.Value;
        if (string.IsNullOrEmpty(assemblyName))
        {
            return excluded;
        }

        var typesByFullName = new Dictionary<string, TypeDefinition>(StringComparer.Ordinal);
        foreach (var type in module.GetAllTypes())
        {
            var fullName = type.FullName;
            if (!string.IsNullOrEmpty(fullName))
            {
                typesByFullName[fullName] = type;
            }
        }

        foreach (var resource in module.Resources)
        {
            if (resource.IsEmbedded == false)
            {
                continue;
            }
            var name = resource.Name?.Value;
            if (name == null || name.EndsWith(".g.resources", StringComparison.OrdinalIgnoreCase) == false)
            {
                continue;
            }
            byte[]? data;
            try
            {
                data = resource.GetData();
            }
            catch
            {
                continue;
            }
            if (data == null)
            {
                continue;
            }
            try
            {
                CollectFromResources(data, assemblyName!, typesByFullName, excluded);
            }
            catch
            {
                // Best effort: an unreadable .g.resources just means we exclude nothing for it.
            }
        }
#endif
        return excluded;
    }

#if !NETSTANDARD
    private static void CollectFromResources(byte[] resourcesData, string assemblyName,
        Dictionary<string, TypeDefinition> typesByFullName, HashSet<TypeDefinition> excluded)
    {
        using var stream = new MemoryStream(resourcesData, writable: false);
        using var reader = new ResourceReader(stream);
        var enumerator = reader.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Key is not string key
                || key.EndsWith(".baml", StringComparison.OrdinalIgnoreCase) == false)
            {
                continue;
            }
            byte[] baml;
            try
            {
                reader.GetResourceData(key, out _, out var rawData);
                if (rawData == null || rawData.Length <= 4)
                {
                    continue;
                }
                // GetResourceData prefixes a stream value with a 4-byte little-endian length.
                baml = new byte[rawData.Length - 4];
                Array.Copy(rawData, 4, baml, 0, baml.Length);
            }
            catch
            {
                continue;
            }

            BamlSymbolReader parsed;
            try
            {
                parsed = BamlSymbolReader.Read(baml);
            }
            catch
            {
                // Unparseable BAML: leave those symbols alone (no worse than before).
                continue;
            }
            CollectFromBaml(parsed, assemblyName, typesByFullName, excluded);
        }
    }

    private static void CollectFromBaml(BamlSymbolReader baml, string assemblyName,
        Dictionary<string, TypeDefinition> typesByFullName, HashSet<TypeDefinition> excluded)
    {
        bool IsLocalAssembly(ushort assemblyId)
        {
            if (baml.Assemblies.TryGetValue(assemblyId, out var assemblyFullName) == false
                || assemblyFullName == null)
            {
                return false;
            }
            var simpleName = assemblyFullName;
            var comma = simpleName.IndexOf(',');
            if (comma >= 0)
            {
                simpleName = simpleName.Substring(0, comma);
            }
            return string.Equals(simpleName.Trim(), assemblyName, StringComparison.OrdinalIgnoreCase);
        }

        TypeDefinition? ResolveLocal(BamlSymbolReader.BamlTypeRef typeRef)
        {
            if (IsLocalAssembly(typeRef.AssemblyId) == false)
            {
                return null;
            }
            return typesByFullName.TryGetValue(typeRef.FullName, out var definition) ? definition : null;
        }

        // Types named directly in XAML (x:Class types, custom controls, converters, ...).
        foreach (var typeRef in baml.Types.Values)
        {
            var definition = ResolveLocal(typeRef);
            if (definition != null)
            {
                excluded.Add(definition);
            }
        }

        // Members named in XAML (bound properties, event handlers, ...). Excluding the whole owner
        // type is the safe baseline: it also preserves property accessors and event-handler methods.
        foreach (var attribute in baml.Attributes)
        {
            if (baml.Types.TryGetValue(attribute.OwnerTypeId, out var ownerRef) == false)
            {
                continue;
            }
            var owner = ResolveLocal(ownerRef);
            if (owner != null)
            {
                excluded.Add(owner);
            }
        }
    }
#endif
}
