using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Headless IL2CPP build entry for getting real global-metadata.dat + GameAssembly.dll to test #276 against.
// Invoke: Unity.exe -batchmode -nographics -projectPath <proj> -executeMethod CIBuild.BuildIl2cpp -buildTarget Win64
public static class CIBuild
{
    public static void BuildIl2cpp()
    {
        try
        {
            // BuildTargetGroup overload (not NamedBuildTarget): NamedBuildTarget is Unity 2021.2+, and the
            // Build matrix also runs 2019.4/2020.3 where it doesn't exist (CS0103). This overload works on all.
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/TestScene.unity" },
                locationPathName = "Build/Il2cpp/BitMonoTest.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log("[CIBuild] result=" + report.summary.result + " errors=" + report.summary.totalErrors);
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[CIBuild] " + e);
            EditorApplication.Exit(2);
        }
    }
}
