using System;
using UnityEditor;
using UnityEngine;

public class BuildVersionPopup : EditorWindow
{
    static readonly GUIContent sBundleVersionLabel = new GUIContent("Bundle Version");
    static readonly GUIContent sBundleVersionCodeLabel = new GUIContent("Bundle Version Code");
    static readonly GUIContent sBuildButtonLabel = new GUIContent("Build");
    static readonly GUIContent sBuildAndRunButtonLabel = new GUIContent("Build and Run");
    static readonly GUIContent sCancelButtonLabel = new GUIContent("Cancel");
    static readonly GUIContent sTitleLabel = new GUIContent("Release Build Version");

    string mOldVersion;
    int mOldVersionCode;

    Action mBuildAction;
    Action mBuildAndRunAction;

    public static void ShowBuildVersionUtility(Action buildAction, Action buildAndRunAction)
    {
        var window = CreateInstance<BuildVersionPopup>();
        window.minSize = new Vector2(300, 120);
        window.maxSize = new Vector2(300, 120);
        window.titleContent = sTitleLabel;

        window.mOldVersion = PlayerSettings.bundleVersion;
        window.mOldVersionCode = PlayerSettings.Android.bundleVersionCode;

        window.mBuildAction = buildAction;
        window.mBuildAndRunAction = buildAndRunAction;

        window.ShowUtility();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Space(12);

        var bundleVersion = EditorGUILayout.TextField(sBundleVersionLabel, PlayerSettings.bundleVersion);
        var bundleVersionCode = EditorGUILayout.IntField(sBundleVersionCodeLabel, PlayerSettings.Android.bundleVersionCode);

        PlayerSettings.bundleVersion = bundleVersion;
        PlayerSettings.Android.bundleVersionCode = bundleVersionCode;

        GUILayout.Space(36);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(sBuildButtonLabel))
        {
            mBuildAction?.Invoke();
        }

        if (GUILayout.Button(sBuildAndRunButtonLabel))
        {
            mBuildAndRunAction?.Invoke();
        }

        GUILayout.Space(24);

        if (GUILayout.Button(sCancelButtonLabel))
        {
            PlayerSettings.bundleVersion = mOldVersion;
            PlayerSettings.Android.bundleVersionCode = mOldVersionCode;
            Close();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(12);
        EditorGUILayout.EndVertical();
    }
}