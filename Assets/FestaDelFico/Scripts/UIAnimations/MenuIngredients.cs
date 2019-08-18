using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuIngredients : MonoBehaviour
{
	public GameObject[] ingredients;
	public LayoutElement[] elements;

	private int activeItem = -1;

	public void ToggleActive(int index)
	{
		if(activeItem < 0)
		{
			ingredients[index].SetActive(true);
			elements[index].flexibleHeight = 2;
			activeItem = index;
		}
		else if(index == activeItem)
		{
			ingredients[index].SetActive(false);
			elements[index].flexibleHeight = 1;
			activeItem = -1;
		}
		else
		{
			ingredients[activeItem].SetActive(false);
			elements[activeItem].flexibleHeight = 1;
			ingredients[index].SetActive(true);
			elements[index].flexibleHeight = 2;
			activeItem = index;
		}
	}
}
