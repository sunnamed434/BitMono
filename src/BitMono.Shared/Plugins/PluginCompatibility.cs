namespace BitMono.Shared.Plugins;

/// <summary>
/// Decides whether a plugin built against a given BitMono.API version is compatible with the host.
/// BitMono uses standard assembly versioning instead of a custom plugin-version attribute, so this just
/// compares the contract assembly versions. See #227.
/// </summary>
public static class PluginCompatibility
{
    /// <summary>
    /// Returns <c>true</c> when the plugin was built against a NEWER contract (compared on Major.Minor)
    /// than the host provides - it likely uses API this BitMono build doesn't have, so it should be
    /// skipped with a clear warning rather than failing cryptically later. Older or equal contracts are
    /// considered compatible (best effort). A locally-built BitMono reports version 0.0.x, which can't be
    /// compared meaningfully, so the check is skipped (returns <c>false</c>) in that case.
    /// </summary>
    public static bool IsBuiltAgainstNewerContract(System.Version? hostVersion, System.Version? pluginContractVersion)
    {
        if (hostVersion == null || pluginContractVersion == null)
        {
            return false;
        }
        if (hostVersion.Major == 0 && hostVersion.Minor == 0)
        {
            return false;
        }
        if (pluginContractVersion.Major != hostVersion.Major)
        {
            return pluginContractVersion.Major > hostVersion.Major;
        }
        return pluginContractVersion.Minor > hostVersion.Minor;
    }
}
