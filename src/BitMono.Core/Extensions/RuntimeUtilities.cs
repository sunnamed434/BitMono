namespace BitMono.Core.Extensions;

public static class RuntimeUtilities
{
    private static EnvironmentRuntimeInformation _lastRuntimeInformation;

    public static EnvironmentRuntimeInformation GetFrameworkInformation()
    {
        if (_lastRuntimeInformation != null)
        {
            return _lastRuntimeInformation;
        }
        var dotnetFrameworkVersion = Environment.Version;
        var operatingSystem = Environment.OSVersion;
        var bits = IntPtr.Size * 8;
        var monoType = Type.GetType(KnownMonoRuntimes.TypeName);
        var hasMono = monoType != null;
        string monoDisplayName = null;
        if (hasMono)
        {
            var displayName = monoType.GetMethod(KnownMonoRuntimes.GetDisplayName, BindingFlags.NonPublic | BindingFlags.Static);
            if (displayName != null)
            {
                monoDisplayName = displayName.ToString();
            }
        }
        return _lastRuntimeInformation = new EnvironmentRuntimeInformation
        {
            OperatingSystem = operatingSystem,
            NetFrameworkVersion = dotnetFrameworkVersion,
            Bits = bits,
            HasMono = hasMono, 
            MonoType = monoType,
            MonoDisplayName = monoDisplayName
        };
    }
}