using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuCreator : MonoBehaviour
{
    [System.Serializable]
	public struct MenuItem
	{
		public string name;
		public int ticket;
		public string ingredients;
	}

	public RectTransform parent;
	public MenuItem[] menu;
	public GameObject menuItemPrefab;
	public float defaultItemHeight = 180;
	public int nameLine = 20;
	public int ingredientsLine = 43;
	public int nameLineHeight = 52;
	public int ingredientsLineHeight = 60;

	private float yPos;

	[ContextMenu("Generate Menu")]
	void Start()
	{
		int n = menu.Length;
		yPos = 0;

		for (int i = 0; i < n; i++)
		{
			GameObject go = Instantiate<GameObject>(menuItemPrefab);
			go.name = "MenuItem_" + i;
			RectTransform rect = go.GetComponent<RectTransform>();
			rect.parent = parent;

			TMP_Text[] data = go.GetComponentsInChildren<TMP_Text>(true);
			data[0].text = menu[i].name.ToUpper();
			data[1].text = menu[i].ticket + " TICKET";
			data[2].text = menu[i].ingredients;

			int extraIngredientsSpace = menu[i].ingredients.Length / ingredientsLine * ingredientsLineHeight;
			int extraNameSpace = menu[i].name.Length / nameLine * nameLineHeight;

			data[0].rectTransform.sizeDelta += Vector2.up * extraNameSpace;
			data[2].rectTransform.anchoredPosition -= Vector2.up * extraNameSpace;
			data[2].rectTransform.sizeDelta += Vector2.up * extraIngredientsSpace;

			float sizeY = defaultItemHeight + extraNameSpace + extraIngredientsSpace;
			rect.sizeDelta = new Vector2(0, sizeY);
			rect.anchoredPosition = Vector3.down * yPos;
			rect.localScale = Vector3.one;
			yPos += sizeY;
		}

		parent.sizeDelta = new Vector2(parent.sizeDelta.x, yPos);
	}
}
