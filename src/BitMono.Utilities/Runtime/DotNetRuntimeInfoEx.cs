namespace BitMono.Utilities.Runtime;

public static class DotNetRuntimeInfoEx
{
    /// <summary>
    /// Retrieves the description of the .NET runtime (e.g., ".NET Core 3.1", ".NET 5.0").
    /// </summary>
    public static string GetFrameworkDescription()
    {
        return RuntimeInformation.FrameworkDescription;
    }
    /// <summary>
    /// Try to get the .NET Framework version (only works if running on .NET Framework).
    /// </summary>
    public static Version? GetNetFrameworkVersion()
    {
        if (Type.GetType("System.Runtime.Versioning.TargetFrameworkAttribute") == null)
        {
            return null;
        }
        var targetFrameworkAttribute = (TargetFrameworkAttribute)Assembly
            .GetEntryAssembly()
            ?.GetCustomAttribute(typeof(TargetFrameworkAttribute));
        if (targetFrameworkAttribute == null)
        {
            return null;
        }
        var version = targetFrameworkAttribute.FrameworkName;
        if (version.Contains('v') == false)
        {
            return null;
        }
        return new Version(version.Substring(version.IndexOf('v') + 1));
    }
    /// <summary>
    /// Checks if Runtime is .NET (Core).
    /// </summary>
    public static bool IsNetCoreOrLater()
    {
        var frameworkDescription = GetFrameworkDescription();
        return frameworkDescription.StartsWith(".NET Core") ||
               frameworkDescription.StartsWith(".NET ") && char.IsDigit(frameworkDescription[5]);
    }
    /// <summary>
    /// Checks if Runtime is .NET Framework.
    /// </summary>
    public static bool IsNetFramework()
    {
        var frameworkDescription = GetFrameworkDescription();
        return frameworkDescription.StartsWith(".NET Framework");
    }
    /// <summary>
    /// Checks if the application is running on Mono.
    /// </summary>
    public static bool IsRunningOnMono()
    {
        return Type.GetType("Mono.Runtime") != null;
    }
    public static string? GetMonoDisplayName()
    {
        if (IsRunningOnMono() == false)
        {
            return null;
        }
        var monoType = Type.GetType("Mono.Runtime");
        if (monoType == null)
        {
            return null;
        }
        var displayNameMethod = monoType.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
        if (displayNameMethod == null)
        {
            return null;
        }
        return displayNameMethod.Invoke(null, null)?.ToString();
    }
    public static int GetArchitectureBits()
    {
        return IntPtr.Size * 8;
    }
    public static OperatingSystem GetOperatingSystem()
    {
        return Environment.OSVersion;
    }
}