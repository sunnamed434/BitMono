namespace BitMono.CLI.Modules;

internal class ObfuscationNeeds
{
#pragma warning disable CS8618
    public string FileName { get; set; }
    public string FileBaseDirectory { get; set; }
    public string ReferencesDirectoryName { get; set; }
    public string OutputPath { get; set; }
    public ObfuscationNeedsWay Way { get; set; }
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