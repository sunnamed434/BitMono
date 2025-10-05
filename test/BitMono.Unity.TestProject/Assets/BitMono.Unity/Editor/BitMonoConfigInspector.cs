#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace BitMono.Unity.Editor
{
    [CustomEditor(typeof(BitMonoConfiguration))]
    public class BitMonoConfigInspector : UnityEditor.Editor
    {
        private void OnEnable()
        {
            var config = (BitMonoConfiguration)target;
            if (config.UseUnityUIForProtections && (config.ProtectionSettings == null || config.ProtectionSettings.Count == 0))
            {
                config.LoadProtectionsFromFile();
                EditorUtility.SetDirty(config);
            }
        }

        public override void OnInspectorGUI()
        {
            var config = (BitMonoConfiguration)target;

            EditorGUILayout.LabelField("BitMono Obfuscation", EditorStyles.boldLabel);
            config.EnableObfuscation = EditorGUILayout.Toggle(
                new GUIContent("Enable Obfuscation", 
                    "Enable BitMono obfuscation during Unity builds. " +
                    "You can also control this programmatically using EditorPrefs.SetBool(\"BitMono.Obfuscation\", true/false). " +
                    "When disabled, obfuscation will be skipped entirely."), 
                config.EnableObfuscation);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuration Path", EditorStyles.boldLabel);
            
            var effectivePath = string.IsNullOrEmpty(config.ConfigPath) ? config.DetectedConfigPath : config.ConfigPath;
            var isAutoDetected = string.IsNullOrEmpty(config.ConfigPath);
            
            EditorGUI.BeginChangeCheck();
            var newConfigPath = EditorGUILayout.TextField(new GUIContent("Config Path", "Leave empty for auto-detection. Set a custom path to override auto-detection."), config.ConfigPath);
            if (EditorGUI.EndChangeCheck())
            {
                config.ConfigPath = newConfigPath;
                EditorUtility.SetDirty(config);
            }
            
            EditorGUI.BeginDisabledGroup(true);
            var pathLabel = isAutoDetected ? "Effective Path (Auto-detected)" : "Effective Path (Custom)";
            EditorGUILayout.TextField(pathLabel, effectivePath);
            EditorGUI.EndDisabledGroup();
            
            if (isAutoDetected && effectivePath.Contains("Not found"))
            {
                EditorGUILayout.HelpBox("Auto-detection failed. Please set a custom config path or ensure BitMono.CLI is accessible.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Obfuscation Settings", EditorStyles.boldLabel);
            config.ObfuscationTimeoutMinutes = EditorGUILayout.IntField(
                new GUIContent("Timeout (minutes)", 
                    "How long to wait for obfuscation to complete before timing out. " +
                    "Increase this if you have large assemblies or complex protections that take longer to process. " +
                    "Decrease this if obfuscation gets stuck on problematic protections or so. " +
                    "Default: 5 minutes"), 
                config.ObfuscationTimeoutMinutes);
            config.EnableDebugLogging = EditorGUILayout.Toggle(
                new GUIContent("Enable Debug Logging", 
                    "Show detailed BitMono output in the Unity Console during builds. " +
                    "Useful for troubleshooting obfuscation issues, seeing which protections are running, " +
                    "and monitoring the obfuscation process. Only shows when this is enabled."), 
                config.EnableDebugLogging);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Protection Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Toggle("Use Unity UI for Protections", config.UseUnityUIForProtections);
            if (EditorGUI.EndChangeCheck())
            {
                config.UseUnityUIForProtections = newValue;
                if (newValue && (config.ProtectionSettings == null || config.ProtectionSettings.Count == 0))
                {
                    config.LoadProtectionsFromFile();
                }
                EditorUtility.SetDirty(config);
            }

            if (config.UseUnityUIForProtections)
            {
                EditorGUILayout.Space();

                if (config.EnableObfuscation)
                {
                    var enabledProtectionsCount = config.ProtectionSettings?.Count(p => p.Enabled) ?? 0;
                    if (enabledProtectionsCount == 0)
                    {
                        EditorGUILayout.HelpBox("No protections are enabled! Obfuscation will fail. Please enable at least one protection below.", MessageType.Error);
                        EditorGUILayout.Space();
                    }
                }

                var protectionSettingsProperty = serializedObject.FindProperty("ProtectionSettings");
                if (protectionSettingsProperty != null)
                {
                    EditorGUILayout.PropertyField(protectionSettingsProperty, new GUIContent("Protection List"), true);


                    if (protectionSettingsProperty.isExpanded)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Load from File"))
                        {
                            config.LoadProtectionsFromFile();
                            EditorUtility.SetDirty(config);
                            serializedObject.Update();
                        }
                        if (GUILayout.Button("Save to File"))
                        {
                            config.SaveProtectionsToFile();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No protections loaded. Click 'Load from File' to load protections from protections.json.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "Unity UI is disabled. BitMono will use protections.json file directly. " +
                    "Edit the protections.json file in your text editor to configure protections.",
                    MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "BitMono will automatically detect configuration files from BitMono.CLI location. " +
                "You can set a custom path if needed, or leave it empty for auto-detection. " +
                "Protections can be configured either through the Unity UI (recommended - no file writing needed) or by directly editing the protections.json file.",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif