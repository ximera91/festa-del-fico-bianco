using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

public class ARMenuController : MonoBehaviour
{
	public GameObject menuUI;
	public GameObject fitToScanOverlay;
	public float defaultScaleFactor = 0.0001f;
	public float animLength = 1;
	public AnimationCurve animationCurve;
	public MenuAnimation menuAnimation;

	private List<AugmentedImage> tempAugmentedImages = new List<AugmentedImage>();
	private Anchor anchor;
	private bool tracking = false;
	private Transform cam;

	void Start()
	{
		cam = Camera.main.transform;
	}

	void Update()
	{
		Session.GetTrackables<AugmentedImage>(tempAugmentedImages, TrackableQueryFilter.Updated);

		if (Session.Status == SessionStatus.Tracking)
		{
			foreach (AugmentedImage image in tempAugmentedImages)
			{
				if (image.TrackingState == TrackingState.Tracking)
				{
					if (!tracking)
					{
						anchor = image.CreateAnchor(image.CenterPose);
						float scaleFactor = defaultScaleFactor * (image.ExtentX / 0.1f);

						if(!menuUI.activeInHierarchy)
						{
							StartCoroutine(OpenMenu(anchor.transform.position, scaleFactor));
						}
						else
						{
							menuUI.transform.position = anchor.transform.position;
						}
						tracking = true;

						if(PlayerPrefs.GetInt("ARMENU_SEEN", 0) == 0)
						{
							PlayerPrefs.SetInt("ARMENU_SEEN", 1);
						}
					}
				}
				else if (image.TrackingState == TrackingState.Stopped)
				{
					tracking = false;
				}
			}
		}

		foreach (AugmentedImage image in tempAugmentedImages)
		{
			if (image.TrackingState == TrackingState.Tracking)
			{
				fitToScanOverlay.SetActive(false);
				return;
			}
		}

		fitToScanOverlay.SetActive(true);
	}

	[ContextMenu("Play Animation")]
	public void PlayAnimation()
	{
		StartCoroutine(OpenMenu(Vector3.zero, defaultScaleFactor));
	}

	private IEnumerator OpenMenu(Vector3 targetPos, float targetScale)
	{
		float t = 0;
		
		menuUI.transform.position = targetPos;
		menuUI.transform.localScale = Vector3.zero;
		menuUI.transform.rotation = Quaternion.LookRotation(cam.forward, cam.up);
		menuUI.SetActive(true);

		while(t < 1)
		{
			float scale = animationCurve.Evaluate(t) * targetScale;
			menuUI.transform.localScale = Vector3.one * scale;
			t += Time.deltaTime / animLength;
			yield return null;
		}

		menuUI.transform.localScale = Vector3.one * targetScale;
		menuAnimation.gameObject.SetActive(true);

		menuAnimation.Play();
	}
}