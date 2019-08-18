using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuAnimation : MonoBehaviour
{
    public RectTransform[] title;
	public LayoutElement[] menu;
	public LayoutElement[] layoutElements;
	public Graphics menuBackground;
	public GameObject occluder;
	public float titleDelay;
	public float menuDelay;
	public bool playOnAwake;

	public void Start()
	{

	}

	public void Play()
	{
		StartCoroutine(PlayAnimation());
	}

	private IEnumerator PlayAnimation()
	{
		occluder.SetActive(true);

		float t = 0;

		while(t < 1)
		{
			yield return null;
		}
	}
}
