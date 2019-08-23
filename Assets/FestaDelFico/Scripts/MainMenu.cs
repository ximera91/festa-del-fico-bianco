using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Permission = NativeCamera.Permission;

public class MainMenu : MonoBehaviour
{
	public Animation menuAnimation;
	public RectTransform[] UIScalers;
	public LayoutGroup menuLayout;
	public RectOffset menuPaddingAR;
	public RectOffset menuPaddingNoAR;
	public Button arMenuButton;
	public Button photoButton;
	public GameObject arHand;
	public GameObject photoHand;
	public DialogueWindowManager dialogueManager;

	private float notchHeight;
	private bool escapePressed;
	private float escapePressedTime = 0;
	private float escapeTimeout = 2;
	private bool semaphore = false;

	private bool arCoreSupported;
	private bool arCoreOk;

	void Awake()
	{
		#if !UNITY_EDITOR
		arMenuButton.gameObject.SetActive(false);
		StartCoroutine(CheckARCoreSupported());
		#endif
	}

	void Start()
	{
		notchHeight = Screen.height - Screen.safeArea.height;
		Vector2 newSizeDelta = new Vector2(0, -notchHeight);
		Vector2 localPositionDelta = Vector2.down * (notchHeight / 2f);

		foreach (RectTransform r in UIScalers)
		{
			r.sizeDelta = newSizeDelta;
			r.anchoredPosition += localPositionDelta;
		}
	}

	void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			if (!escapePressed)
			{
				Toast.ShowAndroidToastMessage("Tocca due volte Indietro per uscire.");
				escapePressed = true;
				escapePressedTime = Time.realtimeSinceStartup;
			}
			else
			{
				if (Time.realtimeSinceStartup - escapePressedTime < escapeTimeout)
				{
					Application.Quit();
				}
				else
				{
					escapePressed = false;
					escapePressedTime = 0;
				}
			}
		}

		if (escapePressed && Time.realtimeSinceStartup - escapePressedTime > escapeTimeout)
		{
			escapePressed = false;
			escapePressedTime = 0;
		}
	}

	private IEnumerator CheckARCoreSupported()
	{
		AsyncTask<ApkAvailabilityStatus> task = Session.CheckApkAvailability();

		yield return task.WaitForCompletion();

		switch (task.Result)
		{
			case ApkAvailabilityStatus.UnsupportedDeviceNotCapable:
				ARSupported(false);
				break;
			case ApkAvailabilityStatus.SupportedNotInstalled:
			case ApkAvailabilityStatus.SupportedApkTooOld:
			case ApkAvailabilityStatus.SupportedInstalled:
				ARSupported(true);
				break;
			case ApkAvailabilityStatus.UnknownTimedOut:
				dialogueManager.ShowDialogue(
					"L'app sta impiegando troppo tempo per rispondere.\n" +
					"Per favore, controlla la connessione a internet e riavvia l'app.");
				break;
			case ApkAvailabilityStatus.UnknownChecking:
			case ApkAvailabilityStatus.UnknownError:
				dialogueManager.ShowDialogue(
					"L'app ha riscontrato un errore.\n" +
					"Per favore, controlla la connessione a internet e riavvia l'app.");
				break;
			default:
				ARSupported(false);
				break;
		}
	}

	public IEnumerator CheckARCoreUpdated(Action successCallback)
	{
		AsyncTask<ApkAvailabilityStatus> task = Session.CheckApkAvailability();

		yield return task.WaitForCompletion();

		switch (task.Result)
		{
			case ApkAvailabilityStatus.SupportedApkTooOld:
			case ApkAvailabilityStatus.SupportedNotInstalled:
				dialogueManager.ShowDialogue(
					"L'app ha bisogno dell'ultima versione di Google Play Services per AR per funzionare.\n",
					"Installa",
					"Rifiuta",
					InstallARCore);
				break;
			case ApkAvailabilityStatus.SupportedInstalled:
				if (successCallback != null)
				{
					successCallback();
				}
				else
				{
					yield return null;
				}
				break;
		}
	}

	public void InstallARCore()
	{
		StartCoroutine(ApkInstallation());
	}

	private IEnumerator ApkInstallation()
	{
		AsyncTask<ApkInstallationStatus> task = Session.RequestApkInstallation(true);
		yield return new WaitUntil(() => (task.IsComplete));

		switch (task.Result)
		{
			case ApkInstallationStatus.Success:
				break;
			case ApkInstallationStatus.ErrorUserDeclined:
				dialogueManager.ShowDialogue(
					"L'app ha bisogno dell'ultima versione di Google Play Services per AR per funzionare.\n",
					"Installa",
					"Rifiuta",
					InstallARCore);
				break;
			case ApkInstallationStatus.Error:
				dialogueManager.ShowDialogue(
					"L'installazione ha riscontrato un errore.\n" +
					"Per favore, controlla la connessione internet e riavvia l'app.");
				break;
		}
	}

	public void StartSceneChangeAR(int sceneIndex)
	{
		#if !UNITY_EDITOR
		if (!arCoreOk)
		{
			StartCoroutine(CheckARCoreUpdated(() =>
			{
				StartSceneChange(sceneIndex);
			}));
		}
		else
		{
			StartSceneChange(sceneIndex);
		}
		#else
		StartSceneChange(sceneIndex);
		#endif
	}

	public void StartSceneChange(int sceneIndex)
	{
		StartCoroutine(ChangeScene(sceneIndex));
	}

	public void AnimationEnded()
	{
		semaphore = true;
	}

	public void ReadyToShowNotifications()
	{
		#if !UNITY_EDITOR
		if(arCoreSupported)
		{
		#endif
			if(PlayerPrefs.GetInt("ARMENU_SEEN", 0) == 0)
			{
				PlayerPrefs.SetInt("ARMENU_SEEN", 0);
				arHand.SetActive(true);
			}
			else
			{
				arHand.SetActive(false);
				if(PlayerPrefs.GetInt("PHOTOMODE_SEEN", 0) == 0)
				{
					PlayerPrefs.SetInt("PHOTOMODE_SEEN", 0);
					photoHand.SetActive(true);
				}
				else
				{
					photoHand.SetActive(false);
				}
			}
		#if !UNITY_EDITOR
		}
		else
		{
			photoHand.SetActive(false);
			arHand.SetActive(false);
		}
		#endif
	}
	
	private void ARSupported(bool supported)
	{
		arMenuButton.gameObject.SetActive(supported);
		photoButton.gameObject.SetActive(supported); // TODO: Da cambiare per foto

		if (supported)
		{
			menuLayout.padding = menuPaddingAR;
			arMenuButton.onClick.AddListener(() => StartSceneChangeAR(1));
			photoButton.onClick.AddListener(() => StartSceneChangeAR(2));
		}
		else
		{
			menuLayout.padding = menuPaddingNoAR;
			// photoButton.onClick.AddListener(() => StartSceneChange(3));
			// TODO: Da cambiare per foto
		}

		arCoreSupported = supported;
	}

	private IEnumerator ChangeScene(int index)
	{
		menuAnimation.Play("menu_close");
		AsyncOperation loading = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
		loading.allowSceneActivation = false;
		yield return new WaitUntil(() => (semaphore));

		if (loading.progress >= 0.9f)
		{
			loading.allowSceneActivation = true;
		}
		else
		{
			while (loading.progress < 0.9f)
			{
				yield return null;
			}

			loading.allowSceneActivation = true;
		}
	}

	public void TryTakePicture()
	{
		if(NativeCamera.CheckPermission() == Permission.Granted)
		{
			NativeCamera.TakePicture(PictureTaken);
		}
		else
		{
			StartCoroutine(CheckCameraPermission());
		}
	}

	private IEnumerator CheckCameraPermission()
	{
		Permission permission = NativeCamera.CheckPermission();

		if(permission == Permission.Denied)
		{
			if(NativeCamera.CanOpenSettings())
			{
				dialogueManager.ShowDialogue(
					"Non hai autorizzato l'accesso alla fotocamera. " +
					"Vuoi andare alle impostazioni del telefono per autorizzarlo?",
					"OK",
					"NO",
					() => NativeCamera.OpenSettings());
			}
			else
			{
				dialogueManager.ShowDialogue(
					"Non hai autorizzato l'accesso alla fotocamera. " +
					"Per favore, autorizza l'accesso nelle impostazioni del telefono.");
			}

			yield break;
		}
		else if (permission == Permission.ShouldAsk)
		{
			dialogueManager.ShowDialogue(
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
					NativeCamera.CheckPermission() == Permission.Granted || 
					Time.realtimeSinceStartup - t > 6;
			});

		if(permission == Permission.Granted)
		{
			TryTakePicture();
		}
	}

	private void PictureTaken(string path)
	{
		if(path == null || path.Length == 0)
		{
			return;
		}

		Texture2D picture = NativeCamera.LoadImageAtPath(path);
	}

	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	public void Exit()
	{
		Application.Quit();
	}
}