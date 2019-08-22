using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using TMPro;

public class MenuCreatorWindow : EditorWindow
{

	[MenuItem("Tools/Editor/Menu Creator", false, 10)]
	public static void ShowMenuCreator()
	{
		MenuCreatorWindow window = ScriptableObject.CreateInstance<MenuCreatorWindow>();

		window.maxSize = new Vector2(400, 1024);
		window.minSize = new Vector2(200, 512);

		window.titleContent = new GUIContent("Menu Creator");

		window.Show();
	}
}
