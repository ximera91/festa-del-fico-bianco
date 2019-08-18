using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

public class ARMenuController : MonoBehaviour
{
	public GameObject menuUI;
	public GameObject fitToScanOverlay;
	public float defaultScaleFactor = 0.0001f;
	[ContextMenuItem("Play Animation", "PlayAnimation")]
	public float animLength = 1;

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
						/* menuUI.SetActive(true);
						menuUI.transform.position = anchor.transform.position;
						menuUI.transform.localScale = Vector3.one * scaleFactor; */

						if(!menuUI.activeInHierarchy)
						{
							StartCoroutine(OpenMenu(anchor.transform.position, scaleFactor));
						}
						tracking = true;

						if(PlayerPrefs.GetInt("ARMENU_SEEN", 0) == 0)
						{
							PlayerPrefs.SetInt("ARMENU_SEEN", 1);
						}
					}
				}
				else if (image.TrackingState == TrackingState.Paused || image.TrackingState == TrackingState.Stopped)
				{
					tracking = false;
				}
			}			

			if (tracking)
			{
				menuUI.transform.rotation = Quaternion.LookRotation(cam.forward, cam.up);
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

	public void PlayAnimation()
	{
		StartCoroutine(OpenMenu(Vector3.zero, defaultScaleFactor));
	}

	private IEnumerator OpenMenu(Vector3 targetPos, float targetScale)
	{
		float t = 0;
		
		menuUI.transform.position = targetPos;
		menuUI.transform.localScale = Vector3.zero;
		menuUI.SetActive(true);

		while(t < 1)
		{
			float scale = EaseOutElastic(0, 1, t) * targetScale;
			menuUI.transform.localScale = Vector3.one * scale;
			t += Time.deltaTime / animLength;
			yield return null;
		}

		menuUI.transform.localScale = Vector3.one * targetScale;

		menuUI.GetComponent<UnityEngine.Playables.PlayableDirector>().Play();
	}

	public float EaseOutElastic(float start, float end, float value)
	{
		end -= start;

		float p = 0.3f;
		float s = p * 0.25f;
		float a = end;

		if (value == 0)
		{
			return start;
		}

		if (value == 1)
		{
			return start + end;
		}

		return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value - s) * (2 * Mathf.PI) / p) + end + start);
	}
}