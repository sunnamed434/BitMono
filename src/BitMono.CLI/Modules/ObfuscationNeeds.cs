namespace BitMono.CLI.Modules;

internal class ObfuscationNeeds
{
#pragma warning disable CS8618
    public string FileName { get; set; }
    public string FileBaseDirectory { get; set; }
    public string ReferencesDirectoryName { get; set; }
    public List<string> ReferencesDirectoryNames { get; set; } = new();
    public string OutputPath { get; set; }
    public ObfuscationNeedsWay Way { get; set; }
    public List<string> Protections { get; set; }
    public ProtectionSettings? ProtectionSettings { get; set; }
    public string? CriticalsFile { get; set; }
    public string? LoggingFile { get; set; }
    public string? ObfuscationFile { get; set; }
    public string? ProtectionsFile { get; set; }
    public ObfuscationSettings? ObfuscationSettings { get; set; }
    // Set when the run is a standalone --inspect-metadata diagnostic instead of an obfuscation. See #276.
    public string? InspectMetadataPath { get; set; }
    // Set when the run is a standalone --encrypt-metadata step instead of an obfuscation. See #276.
    public string? EncryptMetadataPath { get; set; }
#pragma warning restore CS8618
}

/// <summary>
/// The way <see cref="ObfuscationNeeds"/> was created.
/// </summary>
public enum ObfuscationNeedsWay
{
    Unknown,
    Readline,
    Options,
    Other,
}