using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore.Examples.ObjectManipulation;

using GalleryPermission = NativeGallery.Permission;


public class ARPhotoController : PhotoController
{
	public GameObject bottomPanel;
	public Canvas[] canvases;
	public LayerMask cullingMask;
	public Graphic flashPanel;
	public float flashTime = 0.1f;
	public AudioManager audioManager;

	public override void TryTakePicture()
	{
		ManipulationSystem.Instance.enabled = false;
		saved = false;

		if(PlayerPrefs.GetInt("PHOTOMODE_SEEN", 0) == 0)
		{
			PlayerPrefs.SetInt("PHOTOMODE_SEEN", 1);
		}

		if(NativeGallery.CheckPermission() == GalleryPermission.Granted)
		{
			StartCoroutine(TakePicture());
		}
		else
		{
			StartCoroutine(CheckCameraPermission(TryTakePicture));
		}
	}

	public override void DiscardPhoto()
	{
		ManipulationSystem.Instance.enabled = true;
		base.DiscardPhoto();
		bottomPanel.SetActive(true);
	}

	public IEnumerator TakePicture()
	{
		LayerMask prev = Camera.main.cullingMask;

		bool[] active;
		active = new bool[canvases.Length];
		for (int i = 0; i < canvases.Length; i++)
		{
			active[i] = canvases[i].enabled;
			canvases[i].enabled = false;
		}

		Camera.main.cullingMask = cullingMask;	

		yield return StartCoroutine(TakeScreenShot());
		
		for (int i = 0; i < canvases.Length; i++)
		{
			canvases[i].enabled = active[i];
		}

		Camera.main.cullingMask = prev;

		Coroutine flash = StartCoroutine(Flash());

		picture = result;
		photoImage.texture = picture;

		yield return flash;

		bottomPanel.SetActive(false);
		photoPanel.SetActive(true);
	}

	private IEnumerator Flash()
	{
		audioManager.PlayCameraShutter();

		flashPanel.gameObject.SetActive(true);
		float t = 0;
		Color color = new Color(255, 255, 255, 0);
		while (t < 1)
		{
			float alpha = Mathf.Lerp(0, 255, Mathf.Sin(t));
			color.a = alpha;
			flashPanel.color = color;
			t += Time.deltaTime / flashTime;

			yield return new WaitForEndOfFrame();
		}

		color.a = 0;
		flashPanel.color = color;
		flashPanel.gameObject.SetActive(false);
	}
}