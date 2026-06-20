using System.Linq;
using System.Text;
using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.Builder;
using BitMono.IL2CPP;

namespace BitMono.CLI.Modules;

// Standalone --rename-il2cpp-exports step (#276 follow-up): mangle the il2cpp_* exports of a native IL2CPP
// binary (GameAssembly.dll) so dumpers can't find the API by name. The decryptor plugin's GetProcAddress hook
// serves the mangled names back to UnityPlayer with the same key, so the game still boots.
internal static class Il2CppExportRenamerCommand
{
    public static int Run(string path, string? keyHex = null)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"file not found: {path}");
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
            var image = PEImage.FromFile(path);
            if (image.Exports is not { } exports)
            {
                Console.Error.WriteLine("No export directory found - is this a native GameAssembly.dll?");
                return KnownReturnStatuses.Failure;
            }

            var renamed = 0;
            foreach (var symbol in exports.Entries)
            {
                if (symbol.IsByName && symbol.Name is { } name && name.StartsWith(Il2CppExportName.Prefix))
                {
                    symbol.Name = Il2CppExportName.Mangle(key, name);
                    renamed++;
                }
            }
            if (renamed == 0)
            {
                Console.Error.WriteLine("No il2cpp_* exports found to rename.");
                return KnownReturnStatuses.Failure;
            }

            // GetProcAddress binary-searches the export-name table, so it must stay sorted (ASCII) by name -
            // renaming left it ordered by the old il2cpp_ names. Re-sort so UnityPlayer can still resolve them.
            var ordered = exports.Entries
                .OrderBy(symbol => symbol.IsByName ? symbol.Name : string.Empty, StringComparer.Ordinal)
                .ToList();
            exports.Entries.Clear();
            foreach (var symbol in ordered)
            {
                exports.Entries.Add(symbol);
            }

            // Rebuild the native PE (TemplatedPEFileBuilder uses the original as a template). Write to a temp
            // first so a failure can't truncate the real binary.
            var file = image.ToPEFile(new TemplatedPEFileBuilder());
            var temp = path + ".renaming";
            using (var stream = File.Create(temp))
            {
                file.Write(new BinaryStreamWriter(stream));
            }
            File.Copy(temp, path, overwrite: true);
            File.Delete(temp);

            Console.WriteLine($"renamed {renamed} il2cpp_* exports in {Path.GetFileName(path)} (dumpers can't " +
                              "find the API by name; GameAssembly.dll resolves them at runtime).");
            return KnownReturnStatuses.Success;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to rename il2cpp exports: {ex.Message}");
            return KnownReturnStatuses.Failure;
        }
    }

    // null/empty -> the fixed dev key; otherwise 32 hex chars = 16 bytes. Matches MetadataEncryptorCommand so
    // the same per-build key drives both the metadata encryption and the export rename.
    private static byte[] ParseKey(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return Encoding.ASCII.GetBytes("BitMono-IL2CPP!!");
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
}
