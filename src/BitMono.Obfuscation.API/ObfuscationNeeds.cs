namespace BitMono.Obfuscation.API;

public class ObfuscationNeeds
{
    public string FileName { get; set; }
    public string FileBaseDirectory { get; set; }
    public string DependenciesDirectoryName { get; set; }
    public string OutputDirectoryName { get; set; }
}