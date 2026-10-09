using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PimpMyBakkie.EditorTools
{
    public static class PimpMyBakkieBuild
    {
        public static void SetupAndBuildAndroid()
        {
            const string scenePath = "Assets/Scenes/Main.unity";
            const string outputPath = "Builds/Android/PimpMyBakkie.apk";

            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Builds/Android");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!EditorSceneManager.SaveScene(scene, scenePath))
                throw new Exception("Could not save the main scene.");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            PlayerSettings.productName = "PimpMyBakkie";
            PlayerSettings.companyName = "Wiekus Viljoen";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.wiekusviljoen.pimpmybakkie");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.runInBackground = true;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log("Starting PimpMyBakkie Android build...");
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log("Android build result: " + report.summary.result + "; errors: " + report.summary.totalErrors + "; warnings: " + report.summary.totalWarnings);

            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Android build failed. See the Unity build log for details.");

            Debug.Log("APK created at " + Path.GetFullPath(outputPath));
        }
    }
}
