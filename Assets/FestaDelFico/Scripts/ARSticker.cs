using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ARSticker : MonoBehaviour
{
	new Camera camera;
	float speed = 2;

	void Start()
	{
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}

	void Update()
	{
		Quaternion rotation = transform.rotation;
		Quaternion newRotation = Quaternion.LookRotation(camera.transform.forward, camera.transform.up);

		transform.rotation = Quaternion.Slerp(rotation, newRotation, Time.deltaTime * speed);
	}

}
	/* public EventSystem eventSystem;
	public GraphicRaycaster raycaster;

	private Touch[] touches;
	private Vector2 touch0StartPosition;
	private Vector2 touch1StartPosition;
	private bool tapPending = false;
	private bool isSelected = false;

	void Start()
	{
		
	}

	void Update()
	{
		if(Input.touchCount > 0)
		{
			if(Input.touchCount == 1)
			{
				Touch touch = Input.GetTouch(0);
				if(touch.phase == TouchPhase.Began)
				{
					tapPending = true;
					touch0StartPosition = touch.position;
				}
				else if(touch.phase == TouchPhase.Ended)
				{
					if(tapPending)
					{
						PointerEventData eventData = new PointerEventData(eventSystem);
						List<RaycastResult> results = new List<RaycastResult>();
						raycaster.Raycast(eventData, results);

						foreach(RaycastResult r in results)
						{
							if(r.gameObject.tag.Equals("Sticker"))
							{
								if (r.gameObject.GetComponentInParent<Sticker>().gameObject == gameObject)
								{
									isSelected = true;
									break;
								}
							}
						}

						tapPending = false;
					}
				}
				else if (touch.phase == TouchPhase.Moved)
				{
					if(tapPending)
					{
						tapPending = false;
					}

					if (isSelected)
					{
						
					}
				}
			}
		}
	}
}
*/




/* using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.Examples.ObjectManipulation;
using GoogleARCore.Examples.ObjectManipulationInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Sticker : Manipulator
{
	public RectTransform sticker;
	public EventSystem eventSystem;
	public GraphicRaycaster raycaster;
	public GameObject selectionVisualization;
	[Range(0.1f, 10.0f)]
	public float minScale = 0.75f;
	[Range(0.1f, 10.0f)]
	public float maxScale = 1.75f;

	private const float ELASTIC_RATIO_LIMIT = 0.8f;
	private const float SENSITIVITY = 0.75f;
	private const float ELASTICITY = 0.15f;
	private const float ROTATION_RATE_DEGREES_TWIST = 2.5f;
	private const float POSITION_SPEED = 12.0f;
	private const float DIFF_THRESHOLD = 0.0001f;

	private float ScaleDelta
	{
		get
		{
			if (minScale > maxScale)
			{
				Debug.LogError("minScale must be smaller than maxScale.");
				return 0.0f;
			}

			return maxScale - minScale;
		}
	}

	private float ClampedScaleRatio
	{
		get
		{
			return Mathf.Clamp01(currentScaleRatio);
		}
	}

	private float CurrentScale
	{
		get
		{
			float elasticScaleRatio = ClampedScaleRatio + ElasticDelta();
			float elasticScale = minScale + (elasticScaleRatio * ScaleDelta);
			return elasticScale;
		}
	}

	private float currentScaleRatio;
	private bool isScaling;
	private bool isTranslating;
	private bool selected = false;
	private Vector3 oldAnchoredPosition;
	private RectTransform rectTransform;

	void Start()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		currentScaleRatio = (transform.localScale.x - minScale) / ScaleDelta;
	}

	protected override void Update()
	{
		base.Update();
	}

	private void LateUpdate()
	{
		if (!isScaling)
		{
			currentScaleRatio =
				Mathf.Lerp(currentScaleRatio, ClampedScaleRatio, Time.deltaTime * 8.0f);
			float currentScale = CurrentScale;
			transform.localScale = new Vector3(currentScale, currentScale, currentScale);
		}
	}

	protected override bool CanStartManipulationForGesture(TapGesture gesture)
	{
		return true;
	}

	protected override bool CanStartManipulationForGesture(DragGesture gesture)
	{
		if (!IsSelected())
		{
			return false;
		}

		if (gesture.TargetObject != null)
		{
			return false;
		}

		return true;
	}

	protected override bool CanStartManipulationForGesture(PinchGesture gesture)
	{
		if (!IsSelected())
		{
			return false;
		}

		if (gesture.TargetObject != null)
		{
			return false;
		}

		return true;
	}

	protected override bool CanStartManipulationForGesture(TwistGesture gesture)
	{
		if (!IsSelected())
		{
			return false;
		}

		if (gesture.TargetObject != null)
		{
			return false;
		}

		return true;
	}

	protected override void OnStartManipulation(PinchGesture gesture)
	{
		isScaling = true;
		currentScaleRatio = (transform.localScale.x - minScale) / ScaleDelta;
	}

	protected override void OnContinueManipulation(PinchGesture gesture)
	{
		currentScaleRatio +=
			SENSITIVITY * GestureTouchesUtility.PixelsToInches(gesture.GapDelta);

		float currentScale = CurrentScale;
		transform.localScale = new Vector3(currentScale, currentScale, currentScale);

		// If we've tried to scale too far beyond the limit, then cancel the gesture
		// to snap back within the scale range.
		if (currentScaleRatio < -ELASTIC_RATIO_LIMIT ||
			currentScaleRatio > (1.0f + ELASTIC_RATIO_LIMIT))
		{
			gesture.Cancel();
		}
	}

	protected override void OnContinueManipulation(DragGesture gesture)
	{
		isTranslating = true;

		oldAnchoredPosition = sticker.anchoredPosition;
		Vector3 desiredPosition = new Vector3(gesture.Position.x, Screen.height - gesture.Position.y, 0);
		Vector3 newAnchoredPosition = 
			Vector3.Lerp(
				oldAnchoredPosition, 
				desiredPosition,
				Time.deltaTime * POSITION_SPEED);

		float diff = (desiredPosition - newAnchoredPosition).sqrMagnitude;
		if(diff < DIFF_THRESHOLD)
		{
			newAnchoredPosition = desiredPosition;
			isTranslating = false;
		}

		rectTransform.anchoredPosition = newAnchoredPosition;
	}

	protected override void OnContinueManipulation(TwistGesture gesture)
	{
		float rotationAmount = gesture.DeltaRotation * ROTATION_RATE_DEGREES_TWIST;
		transform.Rotate(0.0f, 0.0f, rotationAmount);
	}

	protected override void OnEndManipulation(TapGesture gesture)
	{
		if (gesture.WasCancelled)
		{
			return;
		}

		if (ManipulationSystem.Instance == null)
		{
			return;
		}

		List<RaycastResult> raycastResults = new List<RaycastResult>();
		PointerEventData data = new PointerEventData(eventSystem);
		data.position = gesture.StartPosition;
		data.pressPosition = gesture.StartPosition;
		raycaster.Raycast(data, raycastResults);

		selected = false;

		foreach (RaycastResult res in raycastResults)
		{
			if (res.gameObject.tag.Equals("Sticker"))
			{
				if (res.gameObject == sticker.gameObject)
				{
					Select();
					selected = true;
					break;
				}
			}
		}

		if (!selected)
		{
			Deselect();
		}
	}

	protected override void OnEndManipulation(PinchGesture gesture)
	{
		isScaling = false;
	}

	private float ElasticDelta()
	{
		float overRatio = 0.0f;
		if (currentScaleRatio > 1.0f)
		{
			overRatio = currentScaleRatio - 1.0f;
		}
		else if (currentScaleRatio < 0.0f)
		{
			overRatio = currentScaleRatio;
		}
		else
		{
			return 0.0f;
		}

		return (1.0f - (1.0f / ((Mathf.Abs(overRatio) * ELASTICITY) + 1.0f))) *
			Mathf.Sign(overRatio);
	}

	protected override void OnSelected()
	{
		selectionVisualization.SetActive(true);
	}

	protected override void OnDeselected()
	{
		selectionVisualization.SetActive(false);
	}
} */