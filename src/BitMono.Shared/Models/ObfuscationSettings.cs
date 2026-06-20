namespace BitMono.Shared.Models;

public class ObfuscationSettings
{
    public bool Watermark { get; set; }
    public bool ClearCLI { get; set; }
    public bool ForceObfuscation { get; set; }
    public string ReferencesDirectoryName { get; set; }
    public string OutputDirectoryName { get; set; }

    // Directory scanned for drop-in plugin assemblies that contain custom protections. Relative paths
    // are resolved against BitMono's base directory; absolute paths are used as-is. See #227.
    public string PluginsDirectoryName { get; set; } = "plugins";
    public bool NotifyProtections { get; set; }
    public bool Tips { get; set; } = true;
    public bool WpfBamlRewrite { get; set; } = true;
    public bool NoInliningMethodObfuscationExclude { get; set; }
    public bool SerializableBitObfuscationExclude { get; set; }
    public bool ObfuscationAttributeObfuscationExclude { get; set; }
    public bool ObfuscateAssemblyAttributeObfuscationExclude { get; set; }
    public bool ReflectionMembersObfuscationExclude { get; set; }
    public bool SerializationMembersObfuscationExclude { get; set; }
    public bool StripObfuscationAttributes { get; set; }
    public bool OutputPEImageBuildErrors { get; set; }
    public bool FailOnNoRequiredDependency { get; set; }
    public bool OutputRuntimeMonikerWarnings { get; set; }
    public bool OutputConfigureForNativeCodeWarnings { get; set; }
    public bool OpenFileDestinationInFileExplorer { get; set; }
    public bool SpecificNamespacesObfuscationOnly { get; set; }
    public string[]? SpecificNamespaces { get; set; }
    public string[]? RandomStrings { get; set; }
    public string? StrongNameKeyFile { get; set; }
    public string? OutputFileName { get; set; }

    // User-selected protection preset/level: Custom (default, use protections.json as-is),
    // Minimal, Balanced, or Maximum. Never auto-detected. A CLI --preset overrides this.
    public string? Preset { get; set; }

    // Build for a Unity IL2CPP game: skip protections that aren't IL2CPP-compatible so they don't break the
    // il2cpp.exe build. Set automatically by the Unity integration, or via the CLI --il2cpp flag. See #250.
    public bool IL2CPP { get; set; }
}