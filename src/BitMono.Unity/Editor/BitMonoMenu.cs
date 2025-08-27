#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

namespace BitMono.Unity.Editor
{
    public static class BitMonoMenu
    {
        [MenuItem("Tools/BitMono/Obfuscate Before Build")]
        public static void ObfuscateBeforeBuild()
        {
            var assemblyPath = Path.Combine(Application.dataPath, "..", "Library", "ScriptAssemblies", "Assembly-CSharp.dll");
            
            if (!File.Exists(assemblyPath))
            {
                EditorUtility.DisplayDialog("BitMono", "Assembly-CSharp.dll not found. Please build your project first.", "OK");
                return;
            }

            RunObfuscation(assemblyPath);
        }

        [MenuItem("Tools/BitMono/Settings")]
        public static void OpenSettings()
        {
            EditorUtility.DisplayDialog("BitMono Settings", "Configure obfuscation settings in BitMono.json", "OK");
        }

        private static void RunObfuscation(string assemblyPath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bitmono",
                    Arguments = $"--file \"{assemblyPath}\" --unity --il2cpp",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                EditorUtility.DisplayDialog("BitMono", "Obfuscation completed successfully!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("BitMono Error", $"Obfuscation failed: {output}", "OK");
            }
        }
    }
}
#endif