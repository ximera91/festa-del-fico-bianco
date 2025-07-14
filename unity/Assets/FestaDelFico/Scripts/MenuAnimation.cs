using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimation : MonoBehaviour
{
    public RectTransform[] menuItems;
	public float menuItemAnimLength = 1;
	public float menuItemAnimOffset = 0.2f;
	public bool playOnStart = true;
	public AnimationCurve curve;
	public bool playOnce;

	private float[] startTimes;
	private float offset;
	private float clipLength;
	private float relativeLength;
	private bool played;

	void Awake()
	{
		GenerateAnimation();
	}

	void Start()
	{
		if(playOnStart)
		{
			StartCoroutine(PlayAnimation());
		}
	}

	void OnEnable()
	{
		if(playOnce && played)
		{
			ResetMenu();
		}
	}

	void OnDisable()
	{
		ResetMenu();
	}

	[ContextMenu("Play")]
	public void Play()
	{
		if(playOnce && played)
		{
			ResetMenu();
			return;
		}

		GenerateAnimation();
		StartCoroutine(PlayAnimation());
		
		played = true;
	}

	private void ResetMenu()
	{
		int n = menuItems.Length;

		for(int i = 0; i < n; i++)
		{
			menuItems[i].localScale = Vector3.one;
		}
	}

	private void GenerateAnimation()
	{
		int n = menuItems.Length;
		startTimes = new float[n];
		
		clipLength = menuItemAnimLength + menuItemAnimOffset * n;

		float lastOffset = 0;
		for(int i = 0; i < n; i++)
		{
			startTimes[i] = lastOffset;
			lastOffset += menuItemAnimOffset;
			menuItems[i].localScale = Vector3.zero;
		}
	}

	private IEnumerator PlayAnimation()
	{
		float t = 0;
		int n = menuItems.Length;

		while(t < clipLength)
		{		
			if(!ApplyAnimation(t))
			{
				for(int i = 0; i < n; i++)
				{
					menuItems[i].localScale = Vector3.one;
				}
				break;
			}

			t += Time.deltaTime;
			yield return null;
		}

		menuItems[n - 1].localScale = Vector3.one;
	}

	private bool ApplyAnimation(float t)
	{
		float rt = 0;
		int n = menuItems.Length;

		for(int i = 0; i < n; i++)
		{			
			rt = Mathf.Clamp01(t - startTimes[i]);

			if(rt > menuItemAnimLength)
			{
				continue;
			}
			else if(rt < 0)
			{
				return true;
			}

			if(Mathf.Abs(menuItems[i].anchoredPosition.y) > Screen.height)
			{
				if(t > startTimes[i] + menuItemAnimLength)
				{
					return false;
				}

				return true;
			}			

			if(curve != null)
			{
				menuItems[i].localScale = Vector3.one * curve.Evaluate(rt);
			}
			else
			{
				menuItems[i].localScale = Vector3.one * EaseOutElastic(0, 1, rt);
			}				
		}

		return true;
	}

	private float EaseOutElastic(float start, float end, float value)
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
