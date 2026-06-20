namespace BitMono.IL2CPP.Tests;

public class Il2CppExportNameTests
{
    [Fact]
    public void Mangle_KnownAnswer_LocksTheNativeContract()
    {
        // The native MangleExportName (global_metadata_decrypt.cpp) must produce this exact value for the same
        // key+name, or UnityPlayer's GetProcAddress("il2cpp_init") -> mangled lookup misses and the game won't
        // boot. Cross-checked against the compiled native here in the #276 export-rename work.
        var key = Encoding.ASCII.GetBytes("BitMono-IL2CPP!!");

        Il2CppExportName.Mangle(key, "il2cpp_init").Should().Be("Z6f8fdaf3");
    }

    [Fact]
    public void Mangle_DiffersPerKey()
    {
        var keyA = new byte[16];
        var keyB = new byte[16];
        keyB[0] = 1;

        Il2CppExportName.Mangle(keyA, "il2cpp_init").Should().NotBe(Il2CppExportName.Mangle(keyB, "il2cpp_init"));
    }
}
