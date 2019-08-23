using UnityEngine;
using System.Collections;
using CameraPermission = NativeCamera.Permission;
using GalleryPermission = NativeGallery.Permission;
using UnityEngine.Events;
using UnityEngine.UI;
using System.IO;

// TODO: controllare incosistenza screenshot e picture
public class PhotoController : MonoBehaviour
{
	public Canvas photoCanvas;
	public GameObject photoPanel;
	public RawImage photoImage;
	public AspectRatioFitter fitter; // TODO: Check picture resolution

	[Header("Stickers")]
	public bool canUseStickers = true;
	public GameObject[] toBeDisabled;

	protected StickerManager stickerManager;
	protected Texture2D picture;
	protected Texture2D result;
	protected bool saved = false;

	[SerializeField]
	protected bool shouldCheckPermissionOnStart = false;

	void Start()
	{
		if(shouldCheckPermissionOnStart)
		{
			StartCoroutine(CheckAllPermissions(null, null));
		}

		stickerManager = GameObject.FindObjectOfType<StickerManager>();
		if(stickerManager == null)
		{
			canUseStickers = false;
			Debug.Log("Sticker Manager not found in the scene. Stickers will be disabled.");
		}

		fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
	}

	public void OpenStickers()
	{
		if(canUseStickers)
		{
			stickerManager.OpenStickers();
		}
	}

	public void CloseStickers()
	{
		if(canUseStickers)
		{
			stickerManager.CloseStickers();

			if(stickerManager.IsDirty())
			{
				saved = false;
			}
		}
	}

    public virtual void TryTakePicture()
	{
		saved = false;
		if(stickerManager.HasActiveStickers())
		{
			stickerManager.DeleteAllStickers();
			stickerManager.CloseStickers();
		}

		if(PlayerPrefs.GetInt("PHOTOMODE_SEEN", 0) == 0)
		{
			PlayerPrefs.SetInt("PHOTOMODE_SEEN", 1);
		}

		if(NativeCamera.CheckPermission() == CameraPermission.Granted)
		{
			NativeCamera.TakePicture(PictureTaken);
		}
		else
		{
			StartCoroutine(CheckCameraPermission(TryTakePicture));
		}
	}

	public void TrySavePicture()
	{
		if(NativeGallery.CheckPermission() == GalleryPermission.Granted)
		{
			SavePicture();
		}
		else
		{
			StartCoroutine(CheckGalleryPermission(TryTakePicture));
		}
	}

	public void TrySharePicture()
	{
		if(NativeGallery.CheckPermission() == GalleryPermission.Granted)
		{
			SharePicture();
		}
		else
		{
			StartCoroutine(CheckGalleryPermission(TryTakePicture));
		}
	}

	protected void PictureTaken(string path)
	{
		if(path == null || path.Length == 0)
		{
			return;
		}

		picture = NativeCamera.LoadImageAtPath(path, -1, false);

		if(picture == null)
		{
			Debug.LogError("Something went wrong. Unable to find image at path " + path);
			DialogueWindowManager.Instance.ShowDialogue("Qualcosa è andato storto. Non è stato possibile salvare la foto.");
			return;
		}
		
		photoImage.texture = picture;
		float ratio = (float)picture.width / (float)picture.height;
		fitter.aspectRatio = ratio;
		photoPanel.SetActive(true);
		if(photoCanvas != null)
		{
			photoCanvas.enabled = true;
		}

		StartCoroutine(TakeScreenShot());

		saved = false;
	}

	protected void SavePicture()
	{
		if (stickerManager.IsDirty())
		{
			saved = false;
			StartCoroutine(SaveWithStickers());
			return;		
		}

		if (saved)
		{
			Toast.ShowAndroidToastMessage("La foto è già stata salvata nella Galleria.");
			return;
		}	

		if (result == null || !result.isReadable)
		{
			Debug.Log("####################################################");
			Debug.Log("Porca l'oca");
			StartCoroutine(SaveWithStickers());
			return;
		}

		GalleryPermission perm = NativeGallery.SaveImageToGallery(
			result,
			"Festa del Fico Bianco",
			"festa_del_fico_bianco_{0}.png");

		if(perm == GalleryPermission.Granted)
		{
			Toast.ShowAndroidToastMessage("La foto è stata salvata nella Galleria.");
			saved = true;
		}
	}

	protected void SharePicture()
	{
		if(stickerManager.IsDirty())
		{
			saved = false;
			StartCoroutine(ShareWithStickers());
			return;
		}
		else if (!saved)
		{
			SavePicture();
		}

		if (result == null || !result.isReadable)
		{
			Debug.Log("####################################################");
			Debug.Log("Porca l'oca condivisa");
			StartCoroutine(ShareWithStickers());
			return;
		}

		string filePath = Path.Combine(Application.temporaryCachePath, "shared_img.png");
		File.WriteAllBytes(filePath, result.EncodeToPNG());
		new NativeShare().AddFile(filePath).Share();
	}

	public virtual void DiscardPhoto()
	{
		if(photoCanvas != null)
		{
			photoCanvas.enabled = false;
		}
		photoPanel.SetActive(false);

		stickerManager.DeleteAllStickers();

		photoImage.texture = null;
		saved = false;
		
		Destroy(result);
		if(picture != null && picture != result)
		{
			Destroy(picture);
		}
	}

	public IEnumerator SaveWithStickers()
	{
		stickerManager.OnBeforeSaving();
		yield return StartCoroutine(TakeScreenShot());

		SavePicture();

		yield return null;
	}

	public IEnumerator ShareWithStickers()
	{
		yield return StartCoroutine(SaveWithStickers());
		yield return null;
		SharePicture();
	}

	public IEnumerator TakeScreenShot()
	{
		bool[] active;		
		active = new bool[toBeDisabled.Length];

		for (int i = 0; i < toBeDisabled.Length; i++)
		{
			active[i] = toBeDisabled[i].activeSelf;
			toBeDisabled[i].SetActive(false);
		}	

		yield return new WaitForEndOfFrame();

		result = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		result.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		result.Apply();

		for (int i = 0; i < toBeDisabled.Length; i++)
		{
			toBeDisabled[i].SetActive(active[i]);
		}
	}
	
	protected IEnumerator CheckAllPermissions(
		UnityAction cameraGrantedCallback, 
		UnityAction galleryGrantedCallback)
	{
		yield return StartCoroutine(CheckCameraPermission(cameraGrantedCallback));
		yield return StartCoroutine(CheckGalleryPermission(galleryGrantedCallback));
	}

	protected IEnumerator CheckCameraPermission(UnityAction permissionGrantedCallback)
	{
		CameraPermission permission = NativeCamera.CheckPermission();

		if(permission != CameraPermission.Granted)
		{
			if(permission == CameraPermission.Denied)
			{
				if(NativeCamera.CanOpenSettings())
				{
					DialogueWindowManager.Instance.ShowDialogue(
						"Non hai autorizzato l'accesso alla fotocamera. " +
						"Vuoi andare alle impostazioni del telefono per autorizzarlo?",
						"OK",
						"NO",
						() => NativeCamera.OpenSettings());
				}
				else
				{
					DialogueWindowManager.Instance.ShowDialogue(
						"Non hai autorizzato l'accesso alla fotocamera. " +
						"Per favore, autorizza l'accesso nelle impostazioni del telefono.");
				}

				yield break;
			}
			else if (permission == CameraPermission.ShouldAsk)
			{
				DialogueWindowManager.Instance.ShowDialogue(
					"L'app ha bisogno dell'accesso alla fotocamera per scattare la foto.",
					"OK",
					"",
					() => { permission = NativeCamera.RequestPermission(); });

				yield return new WaitForSeconds(0.1f);
			}

			float t = Time.realtimeSinceStartup;
			yield return new WaitUntil(
				() => 
				{
					return 
						NativeCamera.CheckPermission() == CameraPermission.Granted || 
						Time.realtimeSinceStartup - t > 6;
				});
		}
		
		if(permissionGrantedCallback != null && permission == CameraPermission.Granted)
		{
			permissionGrantedCallback();
		}
	}

	protected IEnumerator CheckGalleryPermission(UnityAction permissionGrantedCallback)
	{
		GalleryPermission permission = NativeGallery.CheckPermission();

		if(permission != GalleryPermission.Granted)
		{
			if(permission == GalleryPermission.Denied)
			{
				if(NativeCamera.CanOpenSettings())
				{
					DialogueWindowManager.Instance.ShowDialogue(
						"Non hai autorizzato l'accesso alla memoria del telefono. " +
						"Vuoi andare alle impostazioni del telefono per autorizzarlo?",
						"OK",
						"NO",
						() => NativeCamera.OpenSettings());
				}
				else
				{
					DialogueWindowManager.Instance.ShowDialogue(
						"Non hai autorizzato l'accesso alla memoria del telefono. " +
						"Per favore, autorizza l'accesso nelle impostazioni del telefono.");
				}

				yield break;
			}
			else if (permission == GalleryPermission.ShouldAsk)
			{
				DialogueWindowManager.Instance.ShowDialogue(
					"L'app ha bisogno dell'accesso alla memoria del telefono per salvare la foto.",
					"OK",
					"",
					() => { permission = NativeGallery.RequestPermission(); });

				yield return new WaitForSeconds(0.1f);
			}

			float t = Time.realtimeSinceStartup;
			yield return new WaitUntil(
				() => 
				{
					return 
						NativeGallery.CheckPermission() == GalleryPermission.Granted || 
						Time.realtimeSinceStartup - t > 6;
				});
		}		

		if(permissionGrantedCallback != null && permission == GalleryPermission.Granted)
		{
			permissionGrantedCallback();
		}
	}
}
