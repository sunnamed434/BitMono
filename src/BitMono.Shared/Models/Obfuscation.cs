namespace BitMono.Shared.Models;

public class Obfuscation
{
    public bool Watermark { get; set; }
    public bool NotifyProtections { get; set; }
    public bool NoInliningMethodObfuscationExclude { get; set; }
    public bool ObfuscationAttributeObfuscationExclude { get; set; }
    public bool FailOnNoRequiredDependency { get; set; }
    public bool OpenFileDestinationInFileExplorer { get; set; }
    public bool SpecificNamespacesObfuscationOnly { get; set; }
    public string[] SpecificNamespaces { get; set; }
    public string[] RandomStrings { get; set; }
}