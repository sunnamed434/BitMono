namespace BitMono.Shared.Models;

/// <summary>
/// User-selected obfuscation preset (protection level). The preset is always chosen
/// explicitly by the user (CLI <c>--preset</c> or obfuscation.json <c>"Preset"</c>);
/// BitMono never infers it from the target runtime. <see cref="Custom"/> means
/// "use protections.json exactly as configured".
/// </summary>
public enum ObfuscationPreset
{
    Custom,
    Minimal,
    Balanced,
    Maximum
}

/// <summary>
/// Maps an <see cref="ObfuscationPreset"/> to the set of protections it enables.
/// The lists below are curated policy and can be tuned freely; the parse/expand
/// mechanism stays the same.
/// </summary>
public static class ProtectionPresets
{
    private static readonly string[] MinimalProtections =
    {
        "FullRenamer",
        "NoNamespaces",
        "BitTimeDateStamp",
    };

    private static readonly string[] BalancedProtections = MinimalProtections.Concat(new[]
    {
        "StringsEncryption",
        "ObjectReturnType",
        "AntiDe4dot",
        "AntiILdasm",
        "BillionNops",
    }).ToArray();

    private static readonly string[] MaximumProtections = BalancedProtections.Concat(new[]
    {
        "AntiDebugBreakpoints",
        "AntiDecompiler",
        "UnmanagedString",
        "DotNetHook",
        "CallToCalli",
        "BitMethodDotnet",
        "BitDecompiler",
        "BitDotNet",
        "BitMono",
    }).ToArray();

    /// <summary>
    /// Parses a preset name (case-insensitive). Unknown/empty values resolve to
    /// <see cref="ObfuscationPreset.Custom"/>.
    /// </summary>
    public static ObfuscationPreset Parse(string? value)
    {
        return Enum.TryParse<ObfuscationPreset>(value, ignoreCase: true, out var preset)
            ? preset
            : ObfuscationPreset.Custom;
    }

    /// <summary>
    /// Expands a preset into the protections it enables, or <c>null</c> for
    /// <see cref="ObfuscationPreset.Custom"/> (the caller keeps using protections.json).
    /// </summary>
    public static ProtectionSettings? Expand(ObfuscationPreset preset)
    {
        var names = preset switch
        {
            ObfuscationPreset.Minimal => MinimalProtections,
            ObfuscationPreset.Balanced => BalancedProtections,
            ObfuscationPreset.Maximum => MaximumProtections,
            _ => null,
        };
        if (names == null)
        {
            return null;
        }
        return new ProtectionSettings
        {
            Protections = names.Select(name => new ProtectionSetting { Name = name, Enabled = true }).ToList()
        };
    }

    /// <summary>
    /// Convenience overload: parse and expand a preset name. Returns <c>null</c> for
    /// Custom/unknown values.
    /// </summary>
    public static ProtectionSettings? Expand(string? presetName)
    {
        return Expand(Parse(presetName));
    }
}
