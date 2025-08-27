#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Diagnostics;

namespace BitMono.Unity.Editor
{
    public class BitMonoBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!EditorPrefs.GetBool("BitMono.AutoObfuscate", false))
                return;

            UnityEngine.Debug.Log("[BitMono] Starting pre-build obfuscation...");
            
            var outputPath = report.summary.outputPath;
            var platform = report.summary.platform;
            
            if (platform == BuildTarget.iOS || platform == BuildTarget.Android)
            {
                ObfuscateForIL2CPP();
            }
            else
            {
                ObfuscateForMono();
            }
        }

        private void ObfuscateForIL2CPP()
        {
            UnityEngine.Debug.Log("[BitMono] Obfuscating for IL2CPP...");
            var assemblies = Directory.GetFiles(Path.Combine(Application.dataPath, "..", "Temp", "StagingArea", "Data", "Managed"), "*.dll");
            
            foreach (var assembly in assemblies)
            {
                if (assembly.Contains("Assembly-CSharp") || assembly.Contains("Assembly-UnityScript"))
                {
                    RunBitMono(assembly, true);
                }
            }
        }

        private void ObfuscateForMono()
        {
            UnityEngine.Debug.Log("[BitMono] Obfuscating for Mono...");
            var assemblyPath = Path.Combine(Application.dataPath, "..", "Library", "ScriptAssemblies", "Assembly-CSharp.dll");
            
            if (File.Exists(assemblyPath))
            {
                RunBitMono(assemblyPath, false);
            }
        }

        private void RunBitMono(string assemblyPath, bool il2cpp)
        {
            var arguments = $"--file \"{assemblyPath}\" --unity";
            if (il2cpp)
                arguments += " --il2cpp";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bitmono",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                UnityEngine.Debug.LogError($"[BitMono] Obfuscation failed: {error}");
            }
            else
            {
                UnityEngine.Debug.Log($"[BitMono] Successfully obfuscated: {Path.GetFileName(assemblyPath)}");
            }
        }
    }
}
#endif