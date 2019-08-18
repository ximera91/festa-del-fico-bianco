using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingAnimation : MonoBehaviour
{
	public Image bg;
	public Image fg;

	public float period = 1;
	
	private float fillAmount = 1;
	private float t = 0;

    void Update()
	{
		t = (t + Time.deltaTime) % (period * 2);
		float coeff = Mathf.Pow(-1, 1 + (int) (t / period));
		fillAmount += Time.deltaTime * coeff;

		fg.fillClockwise = coeff > 0 ? true : false;
		fg.fillAmount = fillAmount;
	}
}
