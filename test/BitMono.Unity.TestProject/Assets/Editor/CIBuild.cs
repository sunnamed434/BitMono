using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif

// Headless IL2CPP build entry for getting real global-metadata.dat + GameAssembly.dll to test #276 against.
// Invoke: Unity.exe -batchmode -nographics -projectPath <proj> -executeMethod CIBuild.BuildIl2cpp -buildTarget Win64
public static class CIBuild
{
    public static void BuildIl2cpp()
    {
        try
        {
            // NamedBuildTarget is Unity 2021.2+; the Build matrix also runs 2019.4/2020.3 where it doesn't
            // exist (CS0103), so fall back to the (future-deprecated) BuildTargetGroup overload there.
            // UNITY_2021_2_OR_NEWER is an auto-defined Unity version symbol (Platform Dependent Compilation).
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
#else
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
#endif
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
