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
    // On an IL2CPP build, encrypt the output global-metadata.dat so static dumpers (Il2CppDumper, Cpp2IL)
    // can't parse it - the decryptor shipped as a source plugin (compiled into GameAssembly.dll on Windows,
    // libil2cpp.so on Android) restores it in memory at startup. A fresh random key is generated per build,
    // baked into the plugin (pre-build) and passed to the encrypt step, so shipped games don't share one key.
    // Windows is handled here (post-build file swap); Android is handled in BitMonoAndroidMetadataProtection
    // (the APK is signed by post-build, so its metadata is encrypted earlier in the Gradle project). Opt-in
    // via BitMonoConfig.EncryptIl2CppMetadata. See #276.
    public class BitMonoMetadataProtection : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        // "BMIL2CPP" little-endian - the plaintext marker at the start of every encrypted file, so it's public
        // anyway (not the key). Read only to skip re-encrypting on an incremental build.
        internal const ulong EncryptedSanity = 0x505043324C494D42UL;

        internal const string KeyHeaderName = "bitmono_il2cpp_key.h";
        // Carries the per-build key from the pre-build step to the encrypt step within the same build.
        internal const string KeyStateKey = "BitMono.Il2cppMetadataKeyHex";
        // The IL2CPP metadata file BitMono encrypts and the native plugin decrypts.
        internal const string GlobalMetadataFileName = "global-metadata.dat";

        public int callbackOrder => 1000;

        // Before the build: bake a fresh random key into the plugin so it compiles into the IL2CPP output.
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
            var platform = report.summary.platform;
            if (platform != BuildTarget.StandaloneWindows64 && platform != BuildTarget.Android)
            {
                return;
            }
            // The Android decryptor is 64-bit only; an ARMv7 slice would ship encrypted metadata with no
            // decryptor and crash on 32-bit devices. Warn loudly rather than break those builds silently.
            if (platform == BuildTarget.Android &&
                (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARMv7) != 0)
            {
                Debug.LogWarning("[BitMono] Encrypt IL2CPP Metadata is on and this Android build targets ARMv7 " +
                                 "(32-bit), which the decryptor doesn't support - ARMv7 devices would crash on the " +
                                 "encrypted metadata. Target ARM64/x86_64 only, or turn the toggle off for ARMv7.");
            }

            var pluginDir = FindPluginDir();
            if (pluginDir == null)
            {
                Debug.LogWarning("[BitMono] Encrypt IL2CPP Metadata is on but the decryptor plugin " +
                                 "(global_metadata_decrypt.cpp) wasn't found; the build won't be protected.");
                return;
            }

            // The key still rides in the binary (obfuscation strength, not a secret), but it differs per game.
            var key = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            File.WriteAllText(Path.Combine(pluginDir, KeyHeaderName), BuildKeyHeader(key));
            SessionState.SetString(KeyStateKey, ToHex(key));
        }

        // After the build: on Windows, encrypt global-metadata.dat with the same key. Always drop the header.
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
            if (report.summary.platform == BuildTarget.StandaloneWindows64)
            {
                var metadata = FindMetadata(report.summary.outputPath);
                if (metadata != null && !IsEncrypted(metadata))
                {
                    RunEncrypt(metadata, keyHex);
                }
            }
            // Android is handled in BitMonoAndroidMetadataProtection (before the APK is packaged + signed).
            // Other platforms have no decryptor, so OnPreprocessBuild wrote no key header - nothing to encrypt.
        }

        // Encrypt metadataPath in place via the CLI (writes <path>.enc, swaps it in). Shared by Windows and
        // Android. keyHex is the per-build key (empty = the CLI's fixed dev key). Returns true on success.
        internal static bool RunEncrypt(string metadataPath, string keyHex)
        {
            var cli = FindCli();
            if (cli == null)
            {
                Debug.LogError("[BitMono] Encrypt IL2CPP Metadata is on but BitMono.CLI.exe wasn't found. " +
                               "global-metadata.dat is left UNENCRYPTED.");
                return false;
            }
            try
            {
                var args = $"--encrypt-metadata \"{metadataPath}\"";
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
                    return false;
                }
                var enc = metadataPath + ".enc";
                if (!File.Exists(enc))
                {
                    Debug.LogError("[BitMono] --encrypt-metadata reported success but produced no .enc file; " +
                                   "global-metadata.dat left UNENCRYPTED.");
                    return false;
                }
                File.Copy(enc, metadataPath, overwrite: true);
                File.Delete(enc);
                Debug.Log("[BitMono] Encrypted IL2CPP global-metadata.dat: static dumpers are blocked; " +
                          "the IL2CPP binary decrypts it at startup.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BitMono] IL2CPP metadata encryption failed: {ex}. " +
                               "global-metadata.dat left UNENCRYPTED.");
                return false;
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
            var expected = Path.Combine(dir, name + "_Data", "il2cpp_data", "Metadata", GlobalMetadataFileName);
            if (File.Exists(expected))
            {
                return expected;
            }
            try
            {
                return Directory.GetFiles(dir, GlobalMetadataFileName, SearchOption.AllDirectories).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        internal static bool IsEncrypted(string path)
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

        private const string CliExeName = "BitMono.CLI.exe";

        internal static string FindCli()
        {
            var dataPath = UnityEngine.Application.dataPath;
            var paths = new[]
            {
                // BitMono.CLI~ is Unity-ignored (~) so its DLLs never ship into the player; the .exe still runs.
                Path.Combine(dataPath, "BitMono.Unity", "BitMono.CLI~", CliExeName),
                Path.Combine(dataPath, "BitMono.Unity", "BitMono.CLI", CliExeName),
                Path.Combine(dataPath, "..", "BitMono.CLI", CliExeName),
                Path.Combine(dataPath, "..", "..", "src", "BitMono.CLI", "bin", "Release", "net462", CliExeName),
            };
            return paths.FirstOrDefault(File.Exists);
        }

        internal static BitMonoConfig LoadConfig()
        {
            var guids = AssetDatabase.FindAssets("t:BitMonoConfig");
            if (guids.Length == 0)
            {
                return null;
            }
            return AssetDatabase.LoadAssetAtPath<BitMonoConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }

#if UNITY_ANDROID
    // Android: encrypt global-metadata.dat in the generated Gradle project, after libil2cpp.so is compiled
    // (with the decryptor + this build's key) but before Gradle packages and signs the APK - you can't swap a
    // file inside a signed APK, so it has to happen here.
    public class BitMonoAndroidMetadataProtection : UnityEditor.Android.IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1000;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var config = BitMonoMetadataProtection.LoadConfig();
            if (config == null || !config.EncryptIl2CppMetadata)
            {
                return;
            }
            var keyHex = SessionState.GetString(BitMonoMetadataProtection.KeyStateKey, "");
            string metadata;
            try
            {
                metadata = Directory.GetFiles(path, BitMonoMetadataProtection.GlobalMetadataFileName, SearchOption.AllDirectories).FirstOrDefault();
            }
            catch
            {
                metadata = null;
            }
            if (metadata == null)
            {
                Debug.LogWarning("[BitMono] Android: global-metadata.dat not found in the Gradle project; " +
                                 "left UNENCRYPTED.");
                return;
            }
            if (BitMonoMetadataProtection.IsEncrypted(metadata))
            {
                return;
            }
            BitMonoMetadataProtection.RunEncrypt(metadata, keyHex);
        }
    }
#endif
}
#endif
