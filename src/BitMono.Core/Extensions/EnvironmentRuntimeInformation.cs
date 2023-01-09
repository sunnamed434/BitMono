namespace BitMono.Core.Extensions;

public class EnvironmentRuntimeInformation
{
    [AllowNull]
    public Version NetFrameworkVersion { get; set; }
    [AllowNull]
    public OperatingSystem OperatingSystem { get; set; }
    [AllowNull]
    public int Bits { get; set; }
    [AllowNull]
    public bool HasMono { get; set; }
    [AllowNull]
    public Type MonoType { get; set; }
    [AllowNull]
    public string MonoDisplayName { get; set; }

    public override string ToString()
    {
        return string.Format("Running on {0}, {1}, x{2} bits", OperatingSystem, HasMono == false
            ? $"{DotNetRuntimeInfo.NetFramework} v{NetFrameworkVersion}"
            : MonoDisplayName, Bits);
    }
}