using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class FicoAppBuilder : IPostprocessBuildWithReport
{
    static BuildPlayerOptions sBuildPlayerOptions;

    public int callbackOrder => 0;

    [MenuItem("Build/Build New Release", priority = 10)]
    public static void BuildNewRelease()
    {
        BuildVersionPopup.ShowBuildVersionUtility(
            () => Build(BuildOptions.ShowBuiltPlayer),
            () => Build(BuildOptions.AutoRunPlayer));
    }

    [MenuItem("Build/Build Current Release", priority = 11)]
    public static void BuildCurrentRelease()
    {
        Build(BuildOptions.ShowBuiltPlayer);
    }

    [MenuItem("Build/Build&Run Current Release", priority = 12)]
    public static void BuildAndRunCurrentRelease()
    {
        Build(BuildOptions.AutoRunPlayer);
    }

    public static void Build(BuildOptions opt)
    {
        if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) || string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
        {
            KeystorePopup.ShowKeystorePopup(() => Build(opt));
            return;
        }

        ConfigureReleaseBuild();

        var buildLocation = GetBuildLocation();

        sBuildPlayerOptions = new BuildPlayerOptions
        {
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = opt,
            locationPathName = buildLocation
        };

        var scenes = EditorBuildSettings.scenes;
        var n = scenes.Length;
        sBuildPlayerOptions.scenes = new string[n];
        for (var i = 0; i < n; i++)
        {
            if (scenes[i].enabled)
            {
                sBuildPlayerOptions.scenes[i] = scenes[i].path;
            }
        }

        BuildPipeline.BuildPlayer(sBuildPlayerOptions);
    }

    private static void ConfigureReleaseBuild()
    {
        EditorUserBuildSettings.buildAppBundle = true;
    }

    private static void ConfigureDevBuild()
    {
        EditorUserBuildSettings.buildAppBundle = false;
    }

    private static string GetBuildLocation()
    {
        var buildDirectory = Path.Combine(Application.dataPath, "..", "output", "builds");
        var buildFileName = "FestaDelFico_" + PlayerSettings.Android.bundleVersionCode + ".abb";
        var buildPath = Path.Combine(buildDirectory, buildFileName);

        if (!Directory.Exists(buildDirectory))
        {
            Directory.CreateDirectory(buildDirectory);
        }

        return buildPath;
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (Application.isBatchMode)
        {
            Debug.Log("Batch mode detected. Skipping post-build configuration.");
            return;
        }

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build Succeeded. Reverting Settings to development configuration.");
            ConfigureDevBuild();
        }
    }
}