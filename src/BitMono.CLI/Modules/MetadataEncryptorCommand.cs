using System.Text;

namespace BitMono.CLI.Modules;

// Standalone --encrypt-metadata step: encrypt a Unity IL2CPP global-metadata.dat so static dumpers can't
// parse it, then self-verify the round-trip. Writes <path>.enc. The matching native decryptor (compiled
// into GameAssembly.dll) uses the same key + format to restore it at runtime.
internal static class MetadataEncryptorCommand
{
    // Fixed dev key, used when --metadata-key isn't given. Must byte-match the key compiled into the native
    // decryptor. The Unity integration passes a random per-build key instead, so shipped games don't share it.
    private static readonly byte[] DefaultKey = Encoding.ASCII.GetBytes("BitMono-IL2CPP!!");

    public static int Run(string path, string? keyHex = null)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"global-metadata.dat not found: {path}");
            return KnownReturnStatuses.Failure;
        }

        byte[] key;
        try
        {
            key = ParseKey(keyHex);
        }
        catch (FormatException ex)
        {
            Console.Error.WriteLine($"Invalid --metadata-key: {ex.Message}");
            return KnownReturnStatuses.Failure;
        }

        try
        {
            var original = File.ReadAllBytes(path);
            var encrypted = GlobalMetadataEncryptor.Encrypt(original, key);
            var outPath = path + ".enc";
            File.WriteAllBytes(outPath, encrypted);

            // Prove it: decrypt restores the original byte-for-byte, and the encrypted file no longer parses.
            var restored = GlobalMetadataEncryptor.Decrypt(encrypted, key);
#if NET6_0_OR_GREATER || NETSTANDARD2_1
            var roundTrips = restored.AsSpan().SequenceEqual(original); // vectorized memcmp
#else
            // Fully-qualified LINQ: on net462 both MonoMod.Backports and System.Memory polyfill the span
            // MemoryExtensions.SequenceEqual, so an unqualified call is ambiguous (CS0121).
            var roundTrips = restored.Length == original.Length && Enumerable.SequenceEqual(restored, original);
#endif
            var stillParses = TryParse(encrypted);

            Console.WriteLine($"global-metadata.dat: {path}");
            Console.WriteLine($"  encrypted: {original.Length:N0} -> {encrypted.Length:N0} bytes -> {outPath}");
            Console.WriteLine($"  round-trip (decrypt restores original byte-for-byte): {(roundTrips ? "OK" : "FAILED")}");
            Console.WriteLine($"  encrypted file parses as metadata: {(stillParses ? "YES (dumpers NOT blocked!)" : "no (dumpers blocked)")}");
            return roundTrips && !stillParses ? KnownReturnStatuses.Success : KnownReturnStatuses.Failure;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to encrypt metadata: {ex.Message}");
            return KnownReturnStatuses.Failure;
        }
    }

    // null/empty -> the fixed dev key; otherwise 32 hex chars = 16 bytes.
    private static byte[] ParseKey(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return DefaultKey;
        }
        hex = hex.Trim();
        if (hex.Length != 32)
        {
            throw new FormatException("expected 32 hex characters (16 bytes).");
        }
        var key = new byte[16];
        for (var i = 0; i < 16; i++)
        {
            key[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return key;
    }

    private static bool TryParse(byte[] data)
    {
        try
        {
            GlobalMetadataFile.Read(data);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
