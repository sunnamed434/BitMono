namespace BitMono.Utilities.Runtime;

public class EnvironmentRuntimeInformation
{
    public Version? NetFrameworkVersion { get; set; }
    public OperatingSystem? OperatingSystem { get; set; }
    public int? Bits { get; set; }
    public bool HasMono { get; set; }
    public string? MonoDisplayName { get; set; }

    public static EnvironmentRuntimeInformation Create()
    {
        return new EnvironmentRuntimeInformation
        {
            NetFrameworkVersion = DotNetRuntimeInfoEx.GetNetFrameworkVersion(),
            OperatingSystem = DotNetRuntimeInfoEx.GetOperatingSystem(),
            Bits = DotNetRuntimeInfoEx.GetArchitectureBits(),
            HasMono = DotNetRuntimeInfoEx.IsRunningOnMono(),
            MonoDisplayName = DotNetRuntimeInfoEx.GetMonoDisplayName()
        };
    }
    /// <summary>
    /// Retrieves detailed information about BitMono runtime, like running OS, .NET Framework/.NET Core version, etc.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("BitMono running on: ");
        stringBuilder.Append($"OS: {OperatingSystem}, ");
        if (HasMono)
        {
            stringBuilder.Append($"{MonoDisplayName} ");
        }
        else
        {
            stringBuilder.Append($"{DotNetRuntimeInfoEx.GetFrameworkDescription()} v{NetFrameworkVersion}, ");
        }
        stringBuilder.Append($"x{Bits} bits");
        return stringBuilder.ToString();
    }
}