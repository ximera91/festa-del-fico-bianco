using UnityEngine;
using UnityEditor;
using System;

public class KeystorePopup : EditorWindow
{
    static readonly GUIContent sKeystorePassLabel = new GUIContent("Keystore Password");
    static readonly GUIContent sKeyaliasPassLabel = new GUIContent("Alias Password");
    static readonly GUIContent sContinueButtonLabel = new GUIContent("Continue");
    static readonly GUIContent sTitleLabel = new GUIContent("Keystore Authentication");

    string mKeystorePass = "";
    string mKeyaliasPass = "";

    internal Action ContinueAction;

    public static void ShowKeystorePopup(Action continueAction)
    {
        var window = CreateInstance<KeystorePopup>();
        window.minSize = new Vector2(300, 120);
        window.maxSize = new Vector2(300, 120);
        window.titleContent = sTitleLabel;

        window.ContinueAction = continueAction;

        window.ShowUtility();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Space(12);

        mKeystorePass = EditorGUILayout.PasswordField(sKeystorePassLabel, mKeystorePass);
        mKeyaliasPass = EditorGUILayout.PasswordField(sKeyaliasPassLabel, mKeyaliasPass);

        PlayerSettings.Android.keystorePass = mKeystorePass;
        PlayerSettings.Android.keyaliasPass = mKeyaliasPass;

        GUILayout.Space(36);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(sContinueButtonLabel))
        {
            ContinueAction?.Invoke();
            Close();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(12);
        EditorGUILayout.EndVertical();
    }
}
