namespace BitMono.Obfuscation.Abstractions;

public class ObfuscationNeeds
{
    public string? FileName { get; set; }
    public string? FileBaseDirectory { get; set; }
    public string? ReferencesDirectoryName { get; set; }
    public string? OutputDirectoryName { get; set; }
}