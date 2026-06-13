namespace BitMono.Core.Attributes;

/// <summary>
/// Marks a protection that cannot run on Unity IL2CPP builds. When BitMono runs in IL2CPP mode
/// (CLI <c>--il2cpp</c> or obfuscation.json <c>"IL2CPP"</c>, set automatically by the Unity
/// integration when the scripting backend is IL2CPP) these protections are skipped.
/// <remarks>
/// On IL2CPP the managed assembly is consumed by <c>il2cpp.exe</c> and converted to C++; native
/// method bodies, <c>calli</c>, PE packers and similar tricks either break that conversion or only
/// affect the managed PE that IL2CPP discards. Pure managed/metadata protections (renaming, string
/// encryption, ...) carry through into <c>global-metadata.dat</c> and are kept. See #250.
/// </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class IL2CPPIncompatibleAttribute : Attribute
{
    public IL2CPPIncompatibleAttribute(string reason = "")
    {
        Reason = reason;
    }

    /// <summary>
    /// Short, user-facing explanation of why the protection does not work on IL2CPP builds.
    /// </summary>
    public string Reason { get; }

    public string GetMessage()
    {
        return string.IsNullOrWhiteSpace(Reason)
            ? "Not supported on IL2CPP builds"
            : Reason;
    }
}
