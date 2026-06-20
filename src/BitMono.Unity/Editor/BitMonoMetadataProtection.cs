#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

namespace BitMono.Unity.Editor
{
    // After an IL2CPP Windows player is built, encrypt its global-metadata.dat in place so static dumpers
    // (Il2CppDumper, Cpp2IL) can't parse it - the decryptor shipped as a source plugin in GameAssembly.dll
    // restores it in memory at startup. Opt-in via BitMonoConfig.EncryptIl2CppMetadata. See #276.
    public class BitMonoMetadataProtection : IPostprocessBuildWithReport
    {
        // "BMIL2CPP" little-endian - the marker GlobalMetadataEncryptor writes; lets us stay idempotent.
        private const ulong EncryptedSanity = 0x505043324C494D42UL;

        public int callbackOrder => 1000; // run last, once the player (and its metadata) is fully written

        public void OnPostprocessBuild(BuildReport report)
        {
            var config = LoadConfig();
            if (config == null || !config.EncryptIl2CppMetadata)
            {
                return;
            }
            // Don't gate on result == Succeeded: OnPostprocessBuild runs before the report is finalized, so the
            // result is usually BuildResult.Unknown here. Only bail on an explicit failure/cancel.
            if (report.summary.result == BuildResult.Failed || report.summary.result == BuildResult.Cancelled)
            {
                return;
            }
            // The native decryptor is Windows x64 only (CreateFileW hook); only encrypt where it can decrypt.
            if (report.summary.platform != BuildTarget.StandaloneWindows64)
            {
                Debug.LogWarning("[BitMono] Encrypt IL2CPP Metadata is on, but it currently supports Windows x64 " +
                                 "only. Leaving global-metadata.dat unencrypted for this platform.");
                return;
            }

            var metadata = FindMetadata(report.summary.outputPath);
            if (metadata == null)
            {
                // No metadata = not an IL2CPP build (Mono backend). Nothing to do.
                return;
            }
            if (IsEncrypted(metadata))
            {
                return; // already done (idempotent for incremental builds)
            }

            var cli = FindCli();
            if (cli == null)
            {
                Debug.LogError("[BitMono] Encrypt IL2CPP Metadata is on but BitMono.CLI.exe wasn't found. " +
                               "global-metadata.dat is left UNENCRYPTED.");
                return;
            }

            try
            {
                // --encrypt-metadata writes <path>.enc and self-verifies the round-trip.
                var psi = new ProcessStartInfo
                {
                    FileName = cli,
                    Arguments = $"--encrypt-metadata \"{metadata}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                using var process = Process.Start(psi);
                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Debug.LogError($"[BitMono] --encrypt-metadata failed (exit {process.ExitCode}); " +
                                   $"global-metadata.dat left UNENCRYPTED.\n{stdout}\n{stderr}");
                    return;
                }

                var enc = metadata + ".enc";
                if (!File.Exists(enc))
                {
                    Debug.LogError("[BitMono] --encrypt-metadata reported success but produced no .enc file; " +
                                   "global-metadata.dat left UNENCRYPTED.");
                    return;
                }
                File.Copy(enc, metadata, overwrite: true);
                File.Delete(enc);
                Debug.Log("[BitMono] Encrypted IL2CPP global-metadata.dat: static dumpers are blocked; " +
                          "GameAssembly.dll decrypts it at startup.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BitMono] IL2CPP metadata encryption failed: {ex}. " +
                               "global-metadata.dat left UNENCRYPTED.");
            }
        }

        // outputPath is the .exe; the metadata sits at <Name>_Data/il2cpp_data/Metadata/global-metadata.dat.
        private static string FindMetadata(string outputPath)
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(dir))
            {
                return null;
            }
            var name = Path.GetFileNameWithoutExtension(outputPath);
            var expected = Path.Combine(dir, name + "_Data", "il2cpp_data", "Metadata", "global-metadata.dat");
            if (File.Exists(expected))
            {
                return expected;
            }
            // Fallback for unusual output layouts.
            try
            {
                return Directory.GetFiles(dir, "global-metadata.dat", SearchOption.AllDirectories).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static bool IsEncrypted(string path)
        {
            try
            {
                using var fs = File.OpenRead(path);
                var head = new byte[8];
                return fs.Read(head, 0, 8) == 8 && BitConverter.ToUInt64(head, 0) == EncryptedSanity;
            }
            catch
            {
                return false;
            }
        }

        private static string FindCli()
        {
            var dataPath = UnityEngine.Application.dataPath;
            var paths = new[]
            {
                // BitMono.CLI~ is Unity-ignored (~) so its DLLs never ship into the player; the .exe still runs.
                Path.Combine(dataPath, "BitMono.Unity", "BitMono.CLI~", "BitMono.CLI.exe"),
                Path.Combine(dataPath, "BitMono.Unity", "BitMono.CLI", "BitMono.CLI.exe"),
                Path.Combine(dataPath, "..", "BitMono.CLI", "BitMono.CLI.exe"),
                Path.Combine(dataPath, "..", "..", "src", "BitMono.CLI", "bin", "Release", "net462", "BitMono.CLI.exe"),
            };
            return paths.FirstOrDefault(File.Exists);
        }

        private static BitMonoConfig LoadConfig()
        {
            var guids = AssetDatabase.FindAssets("t:BitMonoConfig");
            if (guids.Length == 0)
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<BitMonoConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
#endif
