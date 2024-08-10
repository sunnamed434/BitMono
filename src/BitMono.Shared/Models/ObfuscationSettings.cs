namespace BitMono.Shared.Models;

public class ObfuscationSettings
{
    public bool Watermark { get; set; }
    public bool ClearCLI { get; set; }
    public bool ForceObfuscation { get; set; }
    public string ReferencesDirectoryName { get; set; }
    public string OutputDirectoryName { get; set; }
    public bool NotifyProtections { get; set; }
    public bool NoInliningMethodObfuscationExclude { get; set; }
    public bool SerializableBitObfuscationExclude { get; set; }
    public bool ObfuscationAttributeObfuscationExclude { get; set; }
    public bool ObfuscateAssemblyAttributeObfuscationExclude { get; set; }
    public bool ReflectionMembersObfuscationExclude { get; set; }
    public bool StripObfuscationAttributes { get; set; }
    public bool OutputPEImageBuildErrors { get; set; }
    public bool FailOnNoRequiredDependency { get; set; }
    public bool OutputRuntimeMonikerWarnings { get; set; }
    public bool OpenFileDestinationInFileExplorer { get; set; }
    public bool ConfigureForNativeCode { get; set; }
    public bool SpecificNamespacesObfuscationOnly { get; set; }
    public string[]? SpecificNamespaces { get; set; }
    public string[]? RandomStrings { get; set; }
}