#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

namespace BitMono.Unity.Editor
{
    // On an IL2CPP Windows build, encrypt the output global-metadata.dat so static dumpers (Il2CppDumper,
    // Cpp2IL) can't parse it - the decryptor shipped as a source plugin in GameAssembly.dll restores it in
    // memory at startup. A fresh random key is generated per build, baked into the plugin (pre-build) and
    // passed to the encrypt step (post-build), so shipped games don't all share one key. Opt-in via
    // BitMonoConfig.EncryptIl2CppMetadata. See #276.
    public class BitMonoMetadataProtection : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // "BMIL2CPP" little-endian - the plaintext marker at the start of every encrypted file, so it's public
        // anyway (not the key). Read here only to skip re-encrypting on an incremental build.
        private const ulong EncryptedSanity = 0x505043324C494D42UL;

        private const string KeyHeaderName = "bitmono_il2cpp_key.h";
        // Carries the per-build key from OnPreprocessBuild to OnPostprocessBuild within the same build.
        private const string KeyStateKey = "BitMono.Il2cppMetadataKeyHex";

        public int callbackOrder => 1000;

        // Before the build: bake a fresh random key into the plugin so it compiles into GameAssembly.dll.
        public void OnPreprocessBuild(BuildReport report)
        {
            SessionState.EraseString(KeyStateKey);
            // Clear any stale key header so the plugin's compile-time gate reflects the current toggle exactly:
            // with no header the plugin compiles to nothing, so a build with the feature off ships no hook.
            CleanupKeyHeader();
            var config = LoadConfig();
            if (config == null || !config.EncryptIl2CppMetadata)
            {
                return;
            }
            if (report.summary.platform != BuildTarget.StandaloneWindows64)
            {
                return;
            }

            var pluginDir = FindPluginDir();
            if (pluginDir == null)
            {
                Debug.LogWarning("[BitMono] Encrypt IL2CPP Metadata is on but the decryptor plugin " +
                                 "(global_metadata_decrypt.cpp) wasn't found; the build won't be protected.");
                return;
            }

            // The key still rides in GameAssembly.dll (obfuscation strength, not a secret), but it now differs
            // per game instead of being the fixed key shared by every BitMono build.
            var key = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            File.WriteAllText(Path.Combine(pluginDir, KeyHeaderName), BuildKeyHeader(key));
            SessionState.SetString(KeyStateKey, ToHex(key));
        }

        // After the build: encrypt global-metadata.dat with the same key, then drop the generated header.
        public void OnPostprocessBuild(BuildReport report)
        {
            var config = LoadConfig();
            if (config == null || !config.EncryptIl2CppMetadata)
            {
                return;
            }

            // The plugin is compiled by now; grab the key and remove the generated header so it doesn't linger.
            var keyHex = SessionState.GetString(KeyStateKey, "");
            SessionState.EraseString(KeyStateKey);
            CleanupKeyHeader();

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
                // --encrypt-metadata writes <path>.enc and self-verifies the round-trip; key matches the plugin.
                var args = $"--encrypt-metadata \"{metadata}\"";
                if (!string.IsNullOrEmpty(keyHex))
                {
                    args += $" --metadata-key {keyHex}";
                }
                var psi = new ProcessStartInfo
                {
                    FileName = cli,
                    Arguments = args,
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

        // Directory holding the native decryptor plugin - where the generated key header goes so the plugin's
        // #include "bitmono_il2cpp_key.h" picks it up.
        private static string FindPluginDir()
        {
            try
            {
                var cpp = Directory
                    .GetFiles(UnityEngine.Application.dataPath, "global_metadata_decrypt.cpp", SearchOption.AllDirectories)
                    .FirstOrDefault();
                return cpp == null ? null : Path.GetDirectoryName(cpp);
            }
            catch
            {
                return null;
            }
        }

        private static string BuildKeyHeader(byte[] key)
        {
            var bytes = string.Join(",", key.Select(b => "0x" + b.ToString("x2")));
            return "// Auto-generated per build by BitMono - do not edit or commit. This build's metadata key.\n" +
                   "#define BITMONO_IL2CPP_KEY_BYTES " + bytes + "\n";
        }

        private static void CleanupKeyHeader()
        {
            var dir = FindPluginDir();
            if (dir == null)
            {
                return;
            }
            foreach (var file in new[] { Path.Combine(dir, KeyHeaderName), Path.Combine(dir, KeyHeaderName + ".meta") })
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // best-effort cleanup
                }
            }
        }

        private static string ToHex(byte[] bytes) => string.Concat(bytes.Select(b => b.ToString("x2")));

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
