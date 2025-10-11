namespace BitMono.Core.Attributes;

/// <summary>
/// Flags that specify which types of members should be excluded from resolution (obfuscation).
/// Used with the <see cref="DoNotResolveAttribute"/> to control which members are protected from obfuscation.
/// </summary>
[Flags]
public enum MemberInclusionFlags
{
    /// <summary>
    /// Exclude special runtime members from obfuscation (e.g., special methods, properties, or fields used by the runtime).
    /// </summary>
    SpecialRuntime = 0x1,
    
    /// <summary>
    /// Exclude model members from obfuscation (e.g., data models, DTOs, or entities that should preserve their structure).
    /// </summary>
    Model = 0x2,
    
    /// <summary>
    /// Exclude members that are used in reflection from obfuscation (e.g., methods, fields, properties accessed via reflection).
    /// </summary>
    Reflection = 0x4,
}