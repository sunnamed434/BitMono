namespace BitMono.CLI.Modules;

internal class ObfuscationNeeds
{
#pragma warning disable CS8618
    public string FileName { get; set; }
    public string FileBaseDirectory { get; set; }
    public string ReferencesDirectoryName { get; set; }
    public string OutputPath { get; set; }
#pragma warning restore CS8618
}