using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DialogueWindowManager : MonoBehaviour
{
	[SerializeField]
	private Canvas dialogueCanvas;
	[SerializeField]
	private TMP_Text dialogueMessage;
	[SerializeField]
	private Button agreeButton;
	[SerializeField]
	private TMP_Text agreeText;
	[SerializeField]
	private Button declineButton;
	[SerializeField]
	private TMP_Text declineText;

	public bool buttonTextUppercase;

	void Start()
	{
		agreeButton.onClick.RemoveAllListeners();
		agreeButton.onClick.AddListener(CloseWindow);
		declineButton.onClick.RemoveAllListeners();
		declineButton.gameObject.SetActive(false);
	}

	public void ShowDialogue(
		string message, 
		string agreeLabel = "OK", 
		string declineLabel = "",
		UnityAction agreeCallback = null,
		UnityAction declineCallback = null)
	{
		dialogueMessage.text = message;
		
		if(agreeLabel.Length == 0)
		{
			agreeText.text = "OK";
		}
		else
		{
			if(buttonTextUppercase)
			{
				agreeLabel = agreeLabel.ToUpper();
			}
			agreeText.text = agreeLabel;
		}

		if(declineLabel.Length == 0)
		{
			declineButton.gameObject.SetActive(false);
		}
		else
		{
			if(buttonTextUppercase)
			{
				declineLabel = declineLabel.ToUpper();
			}
			declineButton.gameObject.SetActive(true);
			declineText.text = declineLabel;
		}

		agreeButton.onClick.RemoveAllListeners();
		declineButton.onClick.RemoveAllListeners();

		if(agreeCallback != null)
		{			
			agreeButton.onClick.AddListener(agreeCallback);
		}
		
		agreeButton.onClick.AddListener(CloseWindow);

		if(declineButton.gameObject.activeInHierarchy)
		{
			if(declineCallback != null)
			{
				declineButton.onClick.AddListener(declineCallback);
			}
			declineButton.onClick.AddListener(CloseWindow);
		}
		else
		{
			declineButton.onClick.RemoveAllListeners();
		}

		dialogueCanvas.enabled = true;
	}

	private void CloseWindow()
	{
		dialogueCanvas.enabled = false;
	}
}
