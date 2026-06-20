namespace BitMono.CLI.Modules;

internal class Options
{
    // Not Required: standalone modes like --inspect-metadata don't need it. The obfuscation path reports a
    // missing file itself (File.Exists below), so dropping the parser-level requirement loses nothing.
    [Option('f', "file", Required = false, HelpText = "Set file path.")]
    public string? File { get; set; }

    [Option('l', "libraries", Required = false, HelpText = "Set one or more dependency (libs) directories, e.g. -l \"C:\\a\" \"C:\\b\" (space-separated).")]
    public IEnumerable<string> Libraries { get; set; } = [];

    [Option('o', "output", Required = false, HelpText = "Set output path.")]
    public string? Output { get; set; }

    [Option('p', "protections", Required = false, HelpText = "Set protections list, also can be set via protections.json.")]
    public IEnumerable<string> Protections { get; set; } = [];

    [Option("protections-file", Required = false, HelpText = "Set protections configuration file path.")]
    public string? ProtectionsFile { get; set; }

    [Option("criticals-file", Required = false, HelpText = "Set criticals configuration file path.")]
    public string? CriticalsFile { get; set; }

    [Option("logging-file", Required = false, HelpText = "Set logging configuration file path.")]
    public string? LoggingFile { get; set; }

    [Option("obfuscation-file", Required = false, HelpText = "Set obfuscation configuration file path.")]
    public string? ObfuscationFile { get; set; }

    [Option("plugins", Required = false, HelpText = "Custom plugins directory to load custom protections from (overrides PluginsDirectoryName in obfuscation.json). Relative paths resolve against BitMono's base directory.")]
    public string? Plugins { get; set; }

    [Option("no-watermark", Required = false, HelpText = "Disable watermarking (overrides obfuscation.json setting).")]
    public bool NoWatermark { get; set; }

    [Option("nologo", Required = false, HelpText = "Don't display the BitMono logo (ASCII banner) on startup. Mirrors the dotnet/MSBuild --nologo convention.")]
    public bool NoLogo { get; set; }

    [Option("strong-name-key", Required = false, HelpText = "Path to strong name key (.snk) file for assembly signing.")]
    public string? StrongNameKey { get; set; }

    [Option('n', "output-name", Required = false, HelpText = "Set output file name.")]
    public string? OutputName { get; set; }

    [Option("preset", Required = false, HelpText = "Protection preset/level: Custom, Minimal, Balanced, or Maximum. When not Custom it overrides protections.json (an explicit -p/--protections list still wins; CLI --preset wins over obfuscation.json).")]
    public string? Preset { get; set; }

    [Option("il2cpp", Required = false, HelpText = "Build the assembly for a Unity IL2CPP game: skip protections that aren't IL2CPP-compatible (native code, calli, PE packers, etc.) so the il2cpp.exe build doesn't break. Set automatically by the BitMono Unity integration.")]
    public bool IL2CPP { get; set; }

    [Option("inspect-metadata", Required = false, HelpText = "Parse a Unity IL2CPP global-metadata.dat and print its version + the names/literals it exposes, then exit. Standalone diagnostic; doesn't obfuscate.")]
    public string? InspectMetadata { get; set; }

    [Option("encrypt-metadata", Required = false, HelpText = "Encrypt a Unity IL2CPP global-metadata.dat (writes <path>.enc) so static dumpers can't parse it. Requires the matching native decryptor in GameAssembly.dll.")]
    public string? EncryptMetadata { get; set; }

    [Option("metadata-key", Required = false, HelpText = "16-byte key as 32 hex chars for --encrypt-metadata / --rename-il2cpp-exports. Must match the key compiled into the native decryptor. Omit to use the fixed dev key; the Unity integration passes a random per-build key.")]
    public string? MetadataKey { get; set; }

    [Option("rename-il2cpp-exports", Required = false, HelpText = "Mangle the il2cpp_* exports of a native GameAssembly.dll so dumpers can't find the IL2CPP API by name. Use --metadata-key to match the key compiled into the decryptor; the game resolves them at runtime.")]
    public string? RenameIl2cppExports { get; set; }
}