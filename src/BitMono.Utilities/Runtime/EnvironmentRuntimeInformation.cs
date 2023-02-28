namespace BitMono.Utilities.Runtime;

public class EnvironmentRuntimeInformation
{
    public Version? NetFrameworkVersion { get; set; }
    public OperatingSystem? OperatingSystem { get; set; }
    public int? Bits { get; set; }
    public bool HasMono { get; set; }
    public Type? MonoType { get; set; }
    public string? MonoDisplayName { get; set; }

    public override string ToString()
    {
        return $"Running on {OperatingSystem}, {(HasMono == false ? $"{DotNetRuntimeInfo.NetFramework} v{NetFrameworkVersion}" : $"{MonoDisplayName}")}, x{Bits} bits";
    }
}