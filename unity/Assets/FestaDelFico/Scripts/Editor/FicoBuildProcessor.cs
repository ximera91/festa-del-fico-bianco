using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class FicoBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
	public class BuildVersionUtility : EditorWindow
	{		
		string oldVersion;
		int oldVersionCode;

		public static void ShowBuildVersionUtility()
		{
			BuildVersionUtility window = ScriptableObject.CreateInstance<BuildVersionUtility>();
			window.minSize = new Vector2(300, 120);
			window.maxSize = new Vector2(300, 120);
			window.titleContent = new GUIContent("Release Build Version");

			window.oldVersion = PlayerSettings.bundleVersion;
			window.oldVersionCode = PlayerSettings.Android.bundleVersionCode;

			window.ShowUtility();
		}

		void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Space(12);

			string bundleVersion = 
				EditorGUILayout.TextField(
					new GUIContent("Bundle Version"), 
					PlayerSettings.bundleVersion);

			int bundleVersionCode = 
				EditorGUILayout.IntField(
					new GUIContent("Bundle Version Code"), 
					PlayerSettings.Android.bundleVersionCode);

			PlayerSettings.bundleVersion = bundleVersion;
			PlayerSettings.Android.bundleVersionCode = bundleVersionCode;

			GUILayout.Space(36);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Build"))
			{
				SetupBuildPath();	
				Build(BuildOptions.ShowBuiltPlayer);			
			}

			if(GUILayout.Button("Build and Run"))
			{
				SetupBuildPath();
				Build(BuildOptions.AutoRunPlayer);		
			}

			GUILayout.Space(24);

			if(GUILayout.Button("Cancel"))
			{
				PlayerSettings.bundleVersion = oldVersion;
				PlayerSettings.Android.bundleVersionCode = oldVersionCode;
				Close();
			}
			
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginHorizontal();

			GUILayout.Space(12);
			EditorGUILayout.EndVertical();
		}

		private void SetupBuildPath()
		{
			string currentPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
			int slash = currentPath.LastIndexOf("/") + 1;
			string buildDirectory = currentPath.Substring(0, slash);
			string buildFileName = currentPath.Substring(slash);
			string buildPath = 
				EditorUtility.SaveFilePanel(
					"Choose Build Path", 
					buildDirectory, 
					buildFileName, 
					"aab"); 

			EditorUserBuildSettings.SetBuildLocation(BuildTarget.Android, buildPath);
		}
	}

	public class KeystorePasswordUtility : EditorWindow
	{
		string keystorePass = "";
		string keyaliasPass = "";

		public static void ShowKeystorePasswordUtility()
		{
			KeystorePasswordUtility window = ScriptableObject.CreateInstance<KeystorePasswordUtility>();
			window.minSize = new Vector2(300, 120);
			window.maxSize = new Vector2(300, 120);
			window.titleContent = new GUIContent("Keystore Authentication");

			window.ShowUtility();
		}

		void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Space(12);

			keystorePass = 
				EditorGUILayout.PasswordField(
					new GUIContent("Keystore Password"), 
					keystorePass);

			keyaliasPass = 
				EditorGUILayout.PasswordField(
					new GUIContent("Alias Password"), 
					keyaliasPass);

			PlayerSettings.keystorePass = keystorePass;
			PlayerSettings.keyaliasPass = keyaliasPass;

			GUILayout.Space(36);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Continue"))
			{				
				BuildWithLastConfiguration(failedBuildOptions);
				Close();		
			}
			
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginHorizontal();

			GUILayout.Space(12);
			EditorGUILayout.EndVertical();
		}
	}

	private static BuildPlayerOptions options;
	private static BuildOptions failedBuildOptions;

	public int callbackOrder
	{
		get
		{
			return 0;
		}
	}

	[MenuItem("Build/Build New Release")]
	public static void BuildNewRelease()
	{
		BuildVersionUtility.ShowBuildVersionUtility();
	}

	[MenuItem("Build/Build Current Release")]
	public static void BuildCurrentRelease()
	{
		Build(BuildOptions.ShowBuiltPlayer);
	}

	[MenuItem("Build/Build and Run Current Release")]
	public static void BuildAndRunCurrentRelease()
	{
		Build(BuildOptions.AutoRunPlayer);
	}

	public static void Build(BuildOptions opt)
	{
		options = new BuildPlayerOptions();
		options.target = BuildTarget.Android;
		options.targetGroup = BuildTargetGroup.Android;
		options.options = opt;
		options.locationPathName = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
		int n = scenes.Length;
		options.scenes = new string[n];
		for(int i = 0; i < n; i++)
		{
			if(scenes[i].enabled)
			{
				options.scenes[i] = scenes[i].path;
			}
		}

		ConfigureReleaseBuild();

		BuildPipeline.BuildPlayer(options);
	}

	public static void BuildWithLastConfiguration(BuildOptions opt)
	{
		options = new BuildPlayerOptions();
		options.target = EditorUserBuildSettings.activeBuildTarget;
		options.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
		options.options = opt;
		options.locationPathName = EditorUserBuildSettings.GetBuildLocation(options.target);
		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
		int n = scenes.Length;
		options.scenes = new string[n];
		for(int i = 0; i < n; i++)
		{
			if(scenes[i].enabled)
			{
				options.scenes[i] = scenes[i].path;
			}
		}

		BuildPipeline.BuildPlayer(options);
	}

	private static void ConfigureReleaseBuild()
	{
		PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
		PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

		EditorUserBuildSettings.buildAppBundle = true;
	}

	private static void ConfigureDevBuild()
	{
		PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
		PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;

		EditorUserBuildSettings.buildAppBundle = false;
	}

	public void OnPreprocessBuild(BuildReport report)
	{
		if ((PlayerSettings.keystorePass == null || 
				PlayerSettings.keystorePass.Length == 0) ||
			(PlayerSettings.keyaliasPass == null || 
				PlayerSettings.keyaliasPass.Length == 0))
		{
			failedBuildOptions = report.summary.options;

			Debug.Log("Build Failed. Keystore data is incomplete. Launching Keystore Password Utility.");
			KeystorePasswordUtility.ShowKeystorePasswordUtility();
		}
	}

	public void OnPostprocessBuild(BuildReport report)
	{
		if(report.summary.result == BuildResult.Succeeded)
		{
			Debug.Log("Build Succeeded. Reverting Settings to development configuration.");
			ConfigureDevBuild();
		}		
	}
}