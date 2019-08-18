using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Permission = NativeGallery.Permission;

public class PhotoModeController : MonoBehaviour
{
	public Canvas[] canvases;
	public MonoBehaviour[] toBeDisabled;
	public LayerMask cullingMask;
	public GameObject permissionPanel;
	public GameObject alertPanel;
	public GameObject photoPanel;
	public RawImage photoImage;
	public Graphic flashPanel;
	public float flashTime = 0.1f;
	public AudioManager audioManager;
	public AudioSource source;

	private Texture2D screenShot;

	private bool saved = false;
	private bool hasPermission = false;

	void Start()
	{
		if(NativeGallery.CheckPermission() == Permission.ShouldAsk)
		{
			permissionPanel.SetActive(true);
		}
		else
		{
			hasPermission = true;
		}
	}

	public void AskForPermission(bool startupRequest)
	{
		if(startupRequest)
		{
			permissionPanel.SetActive(false);
		}
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
			alertPanel.SetActive(!startupRequest);
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
		StartCoroutine(Shoot());

		if(PlayerPrefs.GetInt("PHOTOMODE_SEEN", 0) == 0)
		{
			PlayerPrefs.SetInt("PHOTOMODE_SEEN", 1);
		}
	}

	public void SaveScreenShot()
	{
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
		if (!saved)
		{
			SaveScreenShot();
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
	}

	public IEnumerator Shoot()
	{
		foreach (Canvas canvas in canvases)
		{
			canvas.enabled = false;
		}

		LayerMask prev = Camera.main.cullingMask;
		Camera.main.cullingMask = cullingMask;

		yield return new WaitForEndOfFrame();

		screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		screenShot.Apply();

		foreach (Canvas canvas in canvases)
		{
			canvas.enabled = true;
		}
		
		Camera.main.cullingMask = prev;
		Coroutine flash = StartCoroutine(Flash());

		photoImage.texture = screenShot;
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