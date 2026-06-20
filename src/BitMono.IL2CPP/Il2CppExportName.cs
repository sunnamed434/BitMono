using System.Text;

namespace BitMono.IL2CPP;

/// <summary>
/// Deterministic per-build rename of an IL2CPP API export (#276 follow-up: export renaming). The post-build
/// renamer rewrites each <c>il2cpp_*</c> export in GameAssembly.dll / libil2cpp.so to <see cref="Mangle"/>,
/// and the native <c>GetProcAddress</c>/<c>dlsym</c> hook computes the identical value so UnityPlayer still
/// resolves the function - while a dumper walking the export table only sees the mangled names.
/// FNV-1a over (key ++ name) keeps it keyed to the per-build key, so it differs per game.
/// </summary>
public static class Il2CppExportName
{
    /// <summary>The IL2CPP API exports all start with this prefix; only these get renamed.</summary>
    public const string Prefix = "il2cpp_";

    private const uint FnvOffset = 2166136261;
    private const uint FnvPrime = 16777619;

    // The native side mirrors this exactly (FNV-1a 32-bit, "Z" + 8 lowercase hex). Keep them in lockstep.
    public static string Mangle(byte[] key, string name)
    {
        var hash = FnvOffset;
        foreach (var b in key)
        {
            hash = unchecked((hash ^ b) * FnvPrime);
        }
        foreach (var b in Encoding.ASCII.GetBytes(name))
        {
            hash = unchecked((hash ^ b) * FnvPrime);
        }
        return "Z" + hash.ToString("x8");
    }
}
