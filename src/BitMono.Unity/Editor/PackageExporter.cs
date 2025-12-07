#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BitMono.Editor
{
    public static class PackageExporter
    {
        private static bool _verboseLogging;

        [MenuItem("BitMono/Test Export Package")]
        public static void TestExportPackage()
        {
            // Set test values for local testing
            Environment.SetEnvironmentVariable("VERSION", "test");
            Environment.SetEnvironmentVariable("UNITY_VERSION", Application.unityVersion);

            Debug.Log("=== BitMono Package Export Test ===");
            _verboseLogging = true;
            ExportPackage();
            _verboseLogging = false;
        }

        public static void ExportPackage()
        {
            try
            {
                var version = Environment.GetEnvironmentVariable("VERSION") ?? Environment.GetEnvironmentVariable("PACKAGE_VERSION");
                var unityVersion = Environment.GetEnvironmentVariable("UNITY_VERSION");

                if (string.IsNullOrWhiteSpace(version))
                {
                    Debug.LogError("VERSION/PACKAGE_VERSION environment variable is missing or empty. Aborting export.");
                    EditorApplication.Exit(1);
                    return;
                }

                if (string.IsNullOrWhiteSpace(unityVersion))
                {
                    Debug.LogError("UNITY_VERSION environment variable is missing or empty. Aborting export.");
                    EditorApplication.Exit(1);
                    return;
                }
                
                var outputPath = Path.Combine(
                    Application.dataPath,
                    "..",
                    "..",
                    "..",
                    $"BitMono-Unity-v{version}-Unity{unityVersion}.unitypackage"
                );
                
                const string basePath = "Assets/BitMono.Unity";
                
                var coreFiles = new[]
                {
                    "Editor/BitMonoBuildProcessor.cs",
                    "Editor/BitMonoConfig.cs",
                    "Editor/BitMonoConfigInspector.cs",
                    "Editor/BitMono.Unity.Editor.asmdef",
                    "package.json",
                    "BitMonoConfig.asset"
                };
                
                List<string> assetsToInclude = new List<string>();
                foreach (var file in coreFiles)
                {
                    var assetPath = $"{basePath}/{file}";
                    if (File.Exists(assetPath))
                    {
                        assetsToInclude.Add(assetPath);
                        if (_verboseLogging)
                            Debug.Log($"Including: {assetPath}");
                    }
                    else
                    {
                        Debug.LogWarning($"File not found (skipping): {assetPath}");
                    }
                }
                
                var cliFolder = Path.Combine(Application.dataPath, "BitMono.Unity/BitMono.CLI");
                if (Directory.Exists(cliFolder))
                {
                    var cliFiles = Directory.GetFiles(cliFolder, "*", SearchOption.AllDirectories)
                        .Where(f => !f.EndsWith(".meta"))
                        .Select(f => f.Replace(Application.dataPath, "Assets").Replace("\\", "/"))
                        .ToArray();

                    assetsToInclude.AddRange(cliFiles);
                    var dllCount = cliFiles.Count(f => f.EndsWith(".dll"));
                    Debug.Log($"Including {cliFiles.Length} files from BitMono.CLI folder ({dllCount} DLLs)");

                    // Verbose: list individual DLLs for debugging
                    if (_verboseLogging)
                    {
                        var dllFiles = cliFiles.Where(f => f.EndsWith(".dll")).ToArray();
                        foreach (var dll in dllFiles.Take(10))
                        {
                            Debug.Log($"    - {Path.GetFileName(dll)}");
                        }
                        if (dllFiles.Length > 10)
                        {
                            Debug.Log($"    ... and {dllFiles.Length - 10} more DLLs");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"BitMono.CLI folder not found: {cliFolder}");
                }
                
                if (assetsToInclude.Count == 0)
                {
                    Debug.LogError("No valid assets found to include in package!");
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log($"Total assets to export: {assetsToInclude.Count}");

                // Refresh the AssetDatabase to ensure all newly copied files are imported
                // This is critical for CLI DLLs that are copied during CI before export
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                AssetDatabase.ExportPackage(
                    assetsToInclude.ToArray(),
                    outputPath,
                    ExportPackageOptions.Default
                );
                if (File.Exists(outputPath))
                {
                    var fileInfo = new FileInfo(outputPath);
                    Debug.Log($"Package exported successfully!");
                    Debug.Log($"  Path: {outputPath}");
                    Debug.Log($"  Size: {fileInfo.Length / 1024.0 / 1024.0:F2} MB");
                    Debug.Log($"  Assets: {assetsToInclude.Count}");
                }
                else
                {
                    Debug.LogError($"Package file was not created: {outputPath}");
                    EditorApplication.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to export package: {ex}");
                EditorApplication.Exit(1);
                throw;
            }
        }
    }
}
#endif
