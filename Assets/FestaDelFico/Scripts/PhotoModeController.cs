﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Permission = NativeGallery.Permission;

public class PhotoModeController : MonoBehaviour
{
	public Canvas[] canvases;
	public LayerMask cullingMask;
	public GameObject photoPanel;
	public RawImage photoImage;
	public Graphic flashPanel;
	public float flashTime = 0.1f;
	public AudioManager audioManager;
	public AudioSource source;

	public DialogueWindowManager dialogueManager;

	[Header("Stickers")]
	public GameObject[] toBeDisabled;

	private Texture2D screenShot;
	private bool saved = false;
	private bool hasPermission = false;
	private bool usedStickers = false;
	private bool savedStickers = false;

	void Start()
	{
		if(NativeGallery.CheckPermission() == Permission.ShouldAsk)
		{
			dialogueManager.ShowDialogue(
				"Questa modalità necessita dell'accesso alla " + 
				"memoria del telefono per salvare le foto.",
				"OK",
				"",
				() => AskForPermission(true));
		}
		else
		{
			hasPermission = true;
		}
	}

	public void UseStickers()
	{
		usedStickers = true;
		saved = false;
	}

	public void AskForPermission(bool startupRequest)
	{
		NativeGallery.RequestPermission();
		StartCoroutine(CheckPermission(startupRequest));
	}

	private IEnumerator CheckPermission(bool startupRequest)
	{
		Permission permission;

		while((permission = NativeGallery.CheckPermission()) == Permission.ShouldAsk)
		{
			yield return null;
		}

		if(permission == Permission.Denied)
		{
			if(!startupRequest)
			{
				dialogueManager.ShowDialogue(
					"Senza permesso di accedere alla memoria del telefono, " +
					"l'applicazione non potrà salvare questa foto.",
					"CHIEDI PERMESSI",
					"ESCI",
					() => AskForPermission(false),
					() => DiscardScreenShot());
			}
			hasPermission = false;
		}
		else
		{
			hasPermission = true;
		}
	}

	public void TakeScreenShot()
	{
		saved = false;
		StartCoroutine(Shoot(false));

		if(PlayerPrefs.GetInt("PHOTOMODE_SEEN", 0) == 0)
		{
			PlayerPrefs.SetInt("PHOTOMODE_SEEN", 1);
		}
	}

	public void SaveScreenShot()
	{
		if (usedStickers && !savedStickers)
		{
			StartCoroutine(SaveWithStickers());
			return;
		}

		if (saved)
		{
			Toast.ShowAndroidToastMessage("La foto è già stata salvata nella Galleria.");
			return;
		}

		if(!hasPermission)
		{
			AskForPermission(false);
			saved = false;
			return;
		}

		NativeGallery.Permission perm = NativeGallery.SaveImageToGallery(
			screenShot,
			"Festa del Fico Bianco",
			"festa_del_fico_bianco_{0}.png");

		if(perm == Permission.Granted)
		{
			Toast.ShowAndroidToastMessage("La foto è stata salvata nella Galleria.");
			saved = true;
		}
	}

	public void ShareScreenShot()
	{
		if (!saved && !usedStickers)
		{
			SaveScreenShot();
		}
		else if(usedStickers && !savedStickers)
		{
			ShareWithStickers();
		}

		string filePath = Path.Combine(Application.temporaryCachePath, "shared_img.png");
		File.WriteAllBytes(filePath, screenShot.EncodeToPNG());
		new NativeShare().AddFile(filePath).Share();
	}

	public void DiscardScreenShot()
	{
		photoImage.texture = null;
		Destroy(screenShot);
		photoPanel.SetActive(false);
		usedStickers = false;
		savedStickers = false;
		saved = false;
	}

	public IEnumerator SaveWithStickers()
	{
		yield return StartCoroutine(Shoot(true));
		savedStickers = true;

		SaveScreenShot();
	}

	public IEnumerator ShareWithStickers()
	{
		yield return StartCoroutine(SaveWithStickers());
		ShareScreenShot();
	}

	public IEnumerator Shoot(bool withStickers)
	{
		LayerMask prev = Camera.main.cullingMask;
		bool[] active;

		if(withStickers)
		{
			active = new bool[toBeDisabled.Length];

			for (int i = 0; i < toBeDisabled.Length; i++)
			{
				active[i] = toBeDisabled[i].activeInHierarchy;
				toBeDisabled[i].SetActive(false);
			}
		}
		else
		{
			active = new bool[canvases.Length];
			for (int i = 0; i < canvases.Length; i++)
			{
				active[i] = canvases[i].enabled;
				canvases[i].enabled = false;
			}
			Camera.main.cullingMask = cullingMask;
		}		

		yield return new WaitForEndOfFrame();

		screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		screenShot.Apply();

		if(withStickers)
		{
			for (int i = 0; i < toBeDisabled.Length; i++)
			{
				toBeDisabled[i].SetActive(active[i]);
			}
		}
		else
		{
			for (int i = 0; i < canvases.Length; i++)
			{
				canvases[i].enabled = active[i];
			}
			Camera.main.cullingMask = prev;
		}

		Coroutine flash = StartCoroutine(Flash());

		if(!usedStickers)
		{
			photoImage.texture = screenShot;
		}
		yield return flash;

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