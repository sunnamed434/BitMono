using System.IO;
using BitMono.Core.Renaming;
#if !NETSTANDARD
using System.Resources;
using AsmResolver;
#endif

namespace BitMono.Core.Analyzing.Baml;

/// <summary>
/// Per-module view of the assembly's compiled WPF XAML (BAML in <c>&lt;assembly&gt;.g.resources</c>).
/// <para>
/// It tells the renamer which types/members XAML references (so they aren't broken), and, when
/// rewriting is enabled, renames the XAML-referenced type names and updates the BAML to match.
/// Members of XAML types are always kept (binding paths and the like live in BAML as plain strings),
/// and a type is only renamed when its name appears nowhere as a string value (which would be an
/// <c>{x:Type}</c>/<c>TargetType</c> reference we don't rewrite).
/// </para>
/// </summary>
public sealed class WpfBamlContext
{
    /// <summary>All in-module types referenced by XAML. Their members are always kept.</summary>
    public HashSet<TypeDefinition> XamlTypes { get; } = new();

    /// <summary>Subset of <see cref="XamlTypes"/> whose name can be safely renamed (BAML updated).</summary>
    public HashSet<TypeDefinition> RenamableTypes { get; } = new();

#if !NETSTANDARD
    private readonly List<(TypeInfoRecord Record, TypeDefinition Type)> _typeEdits = new();
    private readonly List<ResourceRewrite> _rewrites = new();
    private bool _canRewrite = true;

    private sealed class ResourceRewrite
    {
        public ManifestResource Resource = null!;
        public Dictionary<string, BamlDocument> Documents = new();
    }
#endif

    public static WpfBamlContext Build(ModuleDefinition module, bool rewriteEnabled)
    {
        var context = new WpfBamlContext();
#if !NETSTANDARD
        var assemblyName = module.Assembly?.Name?.Value;
        if (string.IsNullOrEmpty(assemblyName))
        {
            return context;
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
            if (!resource.IsEmbedded)
            {
                continue;
            }
            var name = resource.Name?.Value;
            if (name == null || !name.EndsWith(".g.resources", StringComparison.OrdinalIgnoreCase))
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
                context.ProcessResource(resource, data, assemblyName!, typesByFullName);
            }
            catch
            {
                // A resource we can't read just means we exclude/rewrite nothing for it.
                context._canRewrite = false;
            }
        }

        context.ComputeRenamable(rewriteEnabled);
#endif
        return context;
    }

#if !NETSTANDARD
    private void ProcessResource(ManifestResource resource, byte[] resourcesData, string assemblyName,
        Dictionary<string, TypeDefinition> typesByFullName)
    {
        var rewrite = new ResourceRewrite { Resource = resource };
        using var stream = new MemoryStream(resourcesData, writable: false);
        using var reader = new ResourceReader(stream);
        var enumerator = reader.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Key is not string key
                || !key.EndsWith(".baml", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            reader.GetResourceData(key, out _, out var rawData);
            if (rawData == null || rawData.Length <= 4)
            {
                continue;
            }
            var baml = new byte[rawData.Length - 4];
            Array.Copy(rawData, 4, baml, 0, baml.Length);

            BamlDocument document;
            try
            {
                document = BamlReader.ReadDocument(new MemoryStream(baml, writable: false));
            }
            catch
            {
                _canRewrite = false;
                continue;
            }
            rewrite.Documents[key] = document;
            CollectFromDocument(document, assemblyName, typesByFullName);
        }
        if (rewrite.Documents.Count > 0)
        {
            _rewrites.Add(rewrite);
        }
    }

    private void CollectFromDocument(BamlDocument document, string assemblyName,
        Dictionary<string, TypeDefinition> typesByFullName)
    {
        var assemblies = new Dictionary<ushort, string>();
        var typeRecords = new Dictionary<ushort, TypeInfoRecord>();
        foreach (var record in document)
        {
            switch (record)
            {
                case AssemblyInfoRecord assembly:
                    assemblies[assembly.AssemblyId] = assembly.AssemblyFullName;
                    break;
                case TypeInfoRecord type:
                    typeRecords[type.TypeId] = type;
                    break;
            }
        }

        bool IsLocal(ushort assemblyId)
        {
            if (!assemblies.TryGetValue(assemblyId, out var full) || full == null)
            {
                return false;
            }
            var simple = full;
            var comma = simple.IndexOf(',');
            if (comma >= 0)
            {
                simple = simple.Substring(0, comma);
            }
            return string.Equals(simple.Trim(), assemblyName, StringComparison.OrdinalIgnoreCase);
        }

        TypeDefinition? Resolve(TypeInfoRecord record)
        {
            if (!IsLocal(record.AssemblyId))
            {
                return null;
            }
            return typesByFullName.TryGetValue(record.TypeFullName ?? string.Empty, out var def) ? def : null;
        }

        foreach (var record in typeRecords.Values)
        {
            var def = Resolve(record);
            if (def != null)
            {
                XamlTypes.Add(def);
                _typeEdits.Add((record, def));
            }
        }
        foreach (var record in document)
        {
            if (record is AttributeInfoRecord attribute
                && typeRecords.TryGetValue(attribute.OwnerTypeId, out var ownerRecord)
                && Resolve(ownerRecord) is { } owner)
            {
                XamlTypes.Add(owner);
            }
        }
    }

    private void ComputeRenamable(bool rewriteEnabled)
    {
        if (!rewriteEnabled || !_canRewrite)
        {
            return;
        }
        // A type whose name appears in any BAML string value is referenced as text (e.g. {x:Type},
        // TargetType, a binding path), which we don't rewrite - keep those names. Only types
        // referenced purely as elements/attributes are safe to rename.
        var stringValues = new List<string>();
        foreach (var rewrite in _rewrites)
        {
            foreach (var document in rewrite.Documents.Values)
            {
                foreach (var record in document)
                {
                    var value = record switch
                    {
                        PropertyRecord p => p.Value,
                        TextRecord t => t.Value,
                        StringInfoRecord s => s.Value,
                        DefAttributeRecord d => d.Value,
                        _ => null
                    };
                    if (!string.IsNullOrEmpty(value))
                    {
                        stringValues.Add(value!);
                    }
                }
            }
        }

        foreach (var type in XamlTypes)
        {
            var simpleName = type.Name?.Value;
            if (string.IsNullOrEmpty(simpleName))
            {
                continue;
            }
            var mentioned = false;
            foreach (var value in stringValues)
            {
                if (value.IndexOf(simpleName!, StringComparison.Ordinal) >= 0)
                {
                    mentioned = true;
                    break;
                }
            }
            if (!mentioned)
            {
                RenamableTypes.Add(type);
            }
        }
    }
#endif

    /// <summary>
    /// Renames the renamable XAML type names (keeping their namespace) and rewrites the BAML so it
    /// still points at them. Members are untouched. No-op when rewriting is disabled/unsafe.
    /// </summary>
    public void ApplyRewrite(ModuleDefinition module, Renamer renamer)
    {
#if !NETSTANDARD
        if (RenamableTypes.Count == 0)
        {
            return;
        }
        foreach (var type in RenamableTypes)
        {
            if (!renamer.CanRename(type))
            {
                continue;
            }
            // BAML resolves a type by its full name (namespace.name), so the new name must not
            // contain '.' - swap the renamer's dots for spaces, which are valid in type names.
            type.Name = renamer.RenameUnsafely().Replace('.', ' ');
        }
        foreach (var (record, type) in _typeEdits)
        {
            record.TypeFullName = type.FullName;
        }
        foreach (var rewrite in _rewrites)
        {
            WriteBack(module, rewrite);
        }
#endif
    }

#if !NETSTANDARD
    private static void WriteBack(ModuleDefinition module, ResourceRewrite rewrite)
    {
        var original = rewrite.Resource.GetData();
        if (original == null)
        {
            return;
        }
        using var output = new MemoryStream();
        using (var writer = new ResourceWriter(output))
        using (var reader = new ResourceReader(new MemoryStream(original, writable: false)))
        {
            var enumerator = reader.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = (string)enumerator.Key;
                reader.GetResourceData(key, out var typeName, out var resourceData);
                if (rewrite.Documents.TryGetValue(key, out var document))
                {
                    using var bamlStream = new MemoryStream();
                    bamlStream.Position = 4;
                    BamlWriter.WriteDocument(document, bamlStream);
                    var length = (int)bamlStream.Length - 4;
                    bamlStream.Position = 0;
                    bamlStream.Write(BitConverter.GetBytes(length), 0, 4);
                    resourceData = bamlStream.ToArray();
                }
                writer.AddResourceData(key, typeName, resourceData);
            }
            writer.Generate();
        }
        var bytes = output.ToArray();
        var index = module.Resources.IndexOf(rewrite.Resource);
        if (index >= 0)
        {
            module.Resources[index] = new ManifestResource(rewrite.Resource.Name,
                rewrite.Resource.Attributes, new DataSegment(bytes));
        }
    }
#endif
}
