using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;

public class BuildScript
{
    public static void BuildAPK()
    {
        string buildPath = Path.Combine("Builds", "parking-game.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            scenes = new[] { "Assets/Scenes/ParkingLot.unity" };
        }

        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
        PlayerSettings.Android.useCustomKeystore = false;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("BUILD SUCCEEDED: " + Path.GetFullPath(buildPath));
            Debug.Log("Size: " + summary.totalSize + " bytes");
        }
        else
        {
            Debug.LogError("BUILD FAILED: " + summary.result);
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Warning)
                        Debug.LogError(msg.content);
                }
            }
            EditorApplication.Exit(1);
        }
    }
}
