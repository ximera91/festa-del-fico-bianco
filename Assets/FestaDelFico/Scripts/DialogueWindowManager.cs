using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DialogueWindowManager : MonoBehaviour
{
	[SerializeField]
	private Canvas dialogueCanvas = null;
	[SerializeField]
	private TMP_Text dialogueMessage = null;
	[SerializeField]
	private Button agreeButton = null;
	[SerializeField]
	private TMP_Text agreeText = null;
	[SerializeField]
	private Button declineButton = null;
	[SerializeField]
	private TMP_Text declineText = null;

	public bool buttonTextUppercase = false;

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
		agreeButton.onClick.RemoveAllListeners();
		declineButton.onClick.RemoveAllListeners();

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

		if(agreeCallback != null)
		{			
			agreeButton.onClick.AddListener(agreeCallback);
		}
		
		agreeButton.onClick.AddListener(CloseWindow);

		if(declineLabel.Length == 0)
		{
			declineButton.gameObject.SetActive(false);
			declineButton.onClick.RemoveAllListeners();
		}
		else
		{
			if(buttonTextUppercase)
			{
				declineLabel = declineLabel.ToUpper();
			}

			declineText.text = declineLabel;

			if(declineCallback != null)
			{
				declineButton.onClick.AddListener(declineCallback);
			}
			declineButton.onClick.AddListener(CloseWindow);

			declineButton.gameObject.SetActive(true);
		}

		dialogueCanvas.enabled = true;
	}

	private void CloseWindow()
	{
		dialogueCanvas.enabled = false;
	}
}
