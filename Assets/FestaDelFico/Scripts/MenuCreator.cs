using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class MenuCreator : MonoBehaviour
{
	[System.Serializable]
	public class Menu
	{
		public MenuItem[] menuItems;

		public int Length
		{
			get
			{
				return menuItems.Length;
			}
		}

		public MenuItem this[int i]
		{
			get => menuItems[i];
			set => menuItems[i] = value;
		}
	}

    [System.Serializable]
	public class MenuItem
	{
		public string name;
		public int ticket;
		public string ingredients;
		public string notes;
	}

	public RectTransform parent;
	public Menu menu;
	public GameObject menuItemPrefab;
	public string menuFileName;
	public string animationFileName;
	public float defaultItemHeight = 180;
	public int nameLine = 20;
	public int ingredientsLine = 43;
	public int nameLineHeight = 52;
	public int ingredientsLineHeight = 60;
	public int notesLineHeight = 84;

	private float yPos;

	void Awake()
	{
		Destroy(this);
	}

	[ContextMenu("Save Menu")]
	private void SaveMenu()
	{
		string json = JsonUtility.ToJson(menu, true);
		File.WriteAllText(GetFilePath(), json);
	}

	[ContextMenu("Load Menu")]
	private void LoadMenu()
	{
		try
		{
			string json = File.ReadAllText(GetFilePath());
			Menu loadedMenu = JsonUtility.FromJson<Menu>(json);
			menu = loadedMenu;
		}
		catch (IOException e)
		{
			Debug.LogException(e);
		}
	}

	private string GetFilePath()
	{
		return Application.dataPath + "/FestaDelFico/Data/" + menuFileName;
	}


	[ContextMenu("Generate Menu")]
	void Generate()
	{
		LoadMenu();

		if(parent.childCount > 0)
		{
			for(int i = parent.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(parent.GetChild(i).gameObject);
			}
		}

		GameObject go;
		RectTransform rect;
		TMP_Text[] ui;
		int extraNotesSpace;
		int extraIngredientsSpace;
		int extraNameSpace;
		float sizeY;
		int n = menu.Length;
		yPos = 0;

		for (int i = 0; i < n; i++)
		{
			extraNameSpace = 0;
			extraIngredientsSpace = 0;
			extraNotesSpace = 0;

			go = Instantiate<GameObject>(menuItemPrefab);
			go.name = "MenuItem_" + i;
			rect = go.GetComponent<RectTransform>();
			rect.SetParent(parent);

			ui = go.GetComponentsInChildren<TMP_Text>(true);

			ui[0].text = menu[i].name.ToUpper();
			extraNameSpace = menu[i].name.Length / nameLine * nameLineHeight;

			ui[1].text = menu[i].ticket + " TICKET";

			if(menu[i].ingredients.Length > 0)
			{				
				ui[2].text = menu[i].ingredients;
				extraIngredientsSpace = menu[i].ingredients.Length / ingredientsLine * ingredientsLineHeight;
			}
			else
			{
				ui[2].rectTransform.gameObject.SetActive(false);
				extraIngredientsSpace = -ingredientsLineHeight;
			}

			if(menu[i].notes.Length > 0)
			{
				ui[3].text = menu[i].notes.ToUpper();
				ui[3].rectTransform.parent.gameObject.SetActive(true);
				extraNotesSpace = notesLineHeight;
			}

			ui[0].rectTransform.sizeDelta += Vector2.up * extraNameSpace;
			ui[2].rectTransform.anchoredPosition -= Vector2.up * extraNameSpace;
			ui[2].rectTransform.sizeDelta += Vector2.up * extraIngredientsSpace;

			sizeY = defaultItemHeight + extraNameSpace + extraIngredientsSpace + extraNotesSpace;
			rect.sizeDelta = new Vector2(0, sizeY);
			rect.anchoredPosition = Vector3.down * yPos;
			rect.localScale = Vector3.one;
			yPos += sizeY;
		}

		parent.sizeDelta = new Vector2(parent.sizeDelta.x, yPos);
	}
}
