using System.Text;

namespace BitMono.CLI.Modules;

// Standalone --encrypt-metadata step: encrypt a Unity IL2CPP global-metadata.dat so static dumpers can't
// parse it, then self-verify the round-trip. Writes <path>.enc. The matching native decryptor (compiled
// into GameAssembly.dll) uses the same key + format to restore it at runtime. See #276.
internal static class MetadataEncryptorCommand
{
    // Demo key: must byte-match the key compiled into the native decryptor stub. 16 bytes (128-bit XXTEA).
    private static readonly byte[] Key = Encoding.ASCII.GetBytes("BitMono-IL2CPP!!");

    public static int Run(string path)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"global-metadata.dat not found: {path}");
            return KnownReturnStatuses.Failure;
        }

        try
        {
            var original = File.ReadAllBytes(path);
            var encrypted = GlobalMetadataEncryptor.Encrypt(original, Key);
            var outPath = path + ".enc";
            File.WriteAllBytes(outPath, encrypted);

            // Prove it: decrypt restores the original byte-for-byte, and the encrypted file no longer parses.
            var restored = GlobalMetadataEncryptor.Decrypt(encrypted, Key);
#if NET6_0_OR_GREATER || NETSTANDARD2_1
            var roundTrips = restored.AsSpan().SequenceEqual(original); // vectorized memcmp
#else
            var roundTrips = restored.Length == original.Length && restored.SequenceEqual(original);
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
