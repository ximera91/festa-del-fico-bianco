using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DigitalRubyShared;

using TouchPhase = DigitalRubyShared.TouchPhase;

public class StickerManager : MonoBehaviour
{
	public GameObject stickersPanel;
	public GameObject stickersButton;
	public GameObject closeButton;
	public GameObject notification;
   	public EventSystem eventSystem;
	public GraphicRaycaster raycaster;
	public GameObject prefab;
	public RectTransform parent;

	private TapGestureRecognizer tapGesture;
	private PanGestureRecognizer panGesture;
	private ScaleGestureRecognizer scaleGesture;
	private RotateGestureRecognizer rotateGesture;
	
	private List<Sticker> activeStickers = new List<Sticker>();
	private Sticker selectedSticker = null;
	private Vector2 positionOffset;
	private PointerEventData eventData;
	private List<RaycastResult> raycastResults;
	private bool isDirty = false;

	void Awake()
	{
		if(prefab.GetComponent<Sticker>() == null)
		{
			Debug.LogError("Sticker prefab must have a Sticker component.");
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
			#endif
		}
		
		positionOffset = new Vector2(Screen.width / 2, Screen.height / 2);

		if(PlayerPrefs.GetInt("STICKERS_SEEN", 0) == 0)
		{
			notification.SetActive(true);
		}
		else
		{
			notification.SetActive(false);
		}
	}

	void Start()
	{
		CreateTapGesture();
		CreatePanGesture();
		CreateScaleGesture();
		CreateRotateGesture();

		scaleGesture.AllowSimultaneousExecution(rotateGesture);
	}

	public void OpenStickers()
	{
		if(PlayerPrefs.GetInt("STICKERS_SEEN", 0) == 0)
		{
			PlayerPrefs.SetInt("STICKERS_SEEN", 1);
			notification.SetActive(false);
		}

		stickersPanel.SetActive(true);
		ToggleButtons();		
	}

	public void CloseStickers()
	{
		notification.SetActive(false);
		stickersPanel.SetActive(false);
		ToggleButtons();
	}

	private void ToggleButtons()
	{
		stickersButton.SetActive(closeButton.activeInHierarchy);
		closeButton.SetActive(!stickersButton.activeInHierarchy);
	}

	private void CreateTapGesture()
	{
		tapGesture = new TapGestureRecognizer();
		tapGesture.StateUpdated += TapGestureCallback;
		tapGesture.RequireGestureRecognizerToFail = panGesture;
		FingersScript.Instance.AddGesture(tapGesture);
	}

	private void TapGestureCallback(GestureRecognizer gesture)
	{
		if (gesture.State == GestureRecognizerState.Ended)
		{
			Vector2 touchPosition = new Vector2(panGesture.FocusX, panGesture.FocusY);
			eventData = new PointerEventData(eventSystem);
			eventData.position = touchPosition;
			eventData.pressPosition = touchPosition;

			raycastResults = new List<RaycastResult>();
			raycaster.Raycast(eventData, raycastResults);

			if(selectedSticker != null)
			{
				selectedSticker.Deselect();
				selectedSticker = null;
			}

			foreach(RaycastResult r in raycastResults)
			{
				if(r.gameObject.tag.Equals("Sticker"))
				{
					selectedSticker = r.gameObject.GetComponent<Sticker>();	
					if(selectedSticker == null)
					{
						selectedSticker = r.gameObject.GetComponentInParent<Sticker>();
					}
					selectedSticker.Select();
					return;		
				}
			}
		}
	}

	private void CreatePanGesture()
	{
		panGesture = new PanGestureRecognizer();
		panGesture.MinimumNumberOfTouchesToTrack = 1;
		panGesture.StateUpdated += PanGestureCallback;
		FingersScript.Instance.AddGesture(panGesture);
	}

	private void PanGestureCallback(GestureRecognizer gesture)
	{
		if(gesture.State == GestureRecognizerState.Began)
		{
			Vector2 touchPosition = new Vector2(panGesture.FocusX, panGesture.FocusY);
			eventData = new PointerEventData(eventSystem);
			eventData.position = touchPosition;
			eventData.pressPosition = touchPosition;

			raycastResults = new List<RaycastResult>();
			raycaster.Raycast(eventData, raycastResults);

			if(selectedSticker != null)
			{
				selectedSticker.Deselect();
				selectedSticker = null;
			}

			foreach(RaycastResult r in raycastResults)
			{
				if(r.gameObject.tag.Equals("Sticker"))
				{
					selectedSticker = r.gameObject.GetComponent<Sticker>();	
					if(selectedSticker == null)
					{
						selectedSticker = r.gameObject.GetComponentInParent<Sticker>();
					}
					selectedSticker.Select();
					return;		
				}
			}
		}
		if (gesture.State == GestureRecognizerState.Executing)
		{
			if(selectedSticker)
			{
				Vector2 touchPosition = new Vector2(panGesture.FocusX, panGesture.FocusY);
				selectedSticker.MoveTo(touchPosition - positionOffset, false);
			}

			isDirty = true;			
		}
	}

	private void CreateScaleGesture()
	{
		scaleGesture = new ScaleGestureRecognizer();
		scaleGesture.StateUpdated += ScaleGestureCallback;
		FingersScript.Instance.AddGesture(scaleGesture);
	}

	private void ScaleGestureCallback(GestureRecognizer gesture)
	{
		if (gesture.State == GestureRecognizerState.Executing)
		{
			selectedSticker.ScaleMultiplier(scaleGesture.ScaleMultiplier);
			isDirty = true;
		}
		else if(gesture.State == GestureRecognizerState.Ended)
		{
			selectedSticker.NormalizeScale();
			isDirty = true;
		}
	}
	private void CreateRotateGesture()
	{
		rotateGesture = new RotateGestureRecognizer();
		rotateGesture.StateUpdated += RotateGestureCallback;
		FingersScript.Instance.AddGesture(rotateGesture);
	}

	private void RotateGestureCallback(GestureRecognizer gesture)
	{
		if (gesture.State == GestureRecognizerState.Executing)
		{
			selectedSticker.rectTransform.Rotate(
				0.0f, 
				0.0f, 
				rotateGesture.RotationRadiansDelta * Mathf.Rad2Deg);
			isDirty = true;
		}
	} 

	public void SpawnNewSticker(Sprite sprite)
	{
		GameObject go = Instantiate(prefab);
		Sticker sticker = go.GetComponent<Sticker>();
		sticker.rectTransform.SetParent(parent, false);

		sticker.rectTransform.anchoredPosition = Vector3.zero;
		sticker.rectTransform.rotation = Quaternion.identity;
		sticker.rectTransform.localScale = Vector3.one;

		sticker.SetSprite(sprite);

		activeStickers.Add(sticker);
		int i = activeStickers.Count - 1;
		sticker.AddListenerDeleteButton(() => DeleteSticker(activeStickers[i]));

		if(selectedSticker != null)
		{
			selectedSticker.Deselect();
		}

		selectedSticker = sticker;
		selectedSticker.Select();
		
		/* foreach (Transform t in sticker.GetComponentInChildren<Transform>(true))
		{	
			fingers.PassThroughObjects.Add(t.gameObject);
		} */

		isDirty = true;
	}

	public bool IsDirty()
	{
		return isDirty;
	}

	public bool HasActiveStickers()
	{
		return activeStickers.Count > 0;
	}

	public void SetDirty(bool value)
	{
		isDirty = value;
	}

	public void DeleteSticker(Sticker sticker)
	{
		if(sticker.IsSelected())
		{
			selectedSticker = null;
		}
		Destroy(sticker.gameObject);

		isDirty = true;
	}

	public void DeleteAllStickers()
	{
		if(activeStickers.Count == 0)
		{
			return;
		}

		foreach(Sticker s in activeStickers)
		{
			Destroy(s.gameObject);
		}		

		FingersScript.Instance.RemoveGesture(tapGesture);
		FingersScript.Instance.RemoveGesture(panGesture);
		FingersScript.Instance.RemoveGesture(scaleGesture);
		FingersScript.Instance.RemoveGesture(rotateGesture);

		isDirty = false;
	}

	public void OnBeforeSaving()
	{
		isDirty = false;
		if(selectedSticker != null)
		{
			selectedSticker.Deselect();
		}
	}
}
