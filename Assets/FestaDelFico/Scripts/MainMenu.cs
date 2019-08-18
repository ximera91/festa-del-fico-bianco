﻿using System;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public Animation menuAnimation;
	public Button[] arButtons;
	public RectTransform[] UIScalers;
	public DialogueWindowManager dialogueManager;

	public GameObject arHand;
	public GameObject photoHand;

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
		foreach (Button b in arButtons)
		{
			b.gameObject.SetActive(false);
		}
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
				foreach (Button b in arButtons)
				{
					b.gameObject.SetActive(false);
				}
				arCoreSupported = false;
				break;
			case ApkAvailabilityStatus.SupportedNotInstalled:
			case ApkAvailabilityStatus.SupportedApkTooOld:
			case ApkAvailabilityStatus.SupportedInstalled:
				foreach (Button b in arButtons)
				{
					b.gameObject.SetActive(true);
				}
				arCoreSupported = true;
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
				foreach (Button b in arButtons)
				{
					b.gameObject.SetActive(false);
				}
				arCoreSupported = false;
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
				arCoreOk = true;
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

	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	public void Exit()
	{
		Application.Quit();
	}
}