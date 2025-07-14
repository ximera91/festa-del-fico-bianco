using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using GoogleARCore.Examples.ObjectManipulation;

public class RemoveManipulator : Manipulator
{
    private Canvas canvas;
	private RectTransform buttonRect;
	private new Camera camera;

	protected override void Update()
	{
		base.Update();

		if (canvas.enabled)
		{
			canvas.transform.LookAt(camera.transform);
		}
	}

	protected override void OnSelected()
	{
		canvas.enabled = true;
	}

	protected override void OnDeselected()
	{
		canvas.enabled = false;
	}

	protected override bool CanStartManipulationForGesture(DragGesture gesture)
	{	
		if(RectTransformUtility.RectangleContainsScreenPoint(buttonRect, gesture.StartPosition))
		{
			Toast.ShowAndroidToastMessage("Mannaggiaandroid");
			gesture.Cancel();
			Remove();
			return false;
		}

		return true;
	} 

	protected override bool CanStartManipulationForGesture(TapGesture gesture)
	{		
		if(RectTransformUtility.RectangleContainsScreenPoint(buttonRect, gesture.StartPosition))
		{
			Toast.ShowAndroidToastMessage("Mannaggiaandroid2 " + buttonRect.name);
			gesture.Cancel();
			Remove();
			return false;
		}

		return true;
	}

	private void Remove()
	{
		Toast.ShowAndroidToastMessage("Remove");
		if (IsSelected())
		{
			Toast.ShowAndroidToastMessage("Selected");
			ManipulationSystem.Instance.Deselect();
			gameObject.SetActive(false);
			Destroy(gameObject);
		}
	}

	public void SetCanvas(Canvas canvas)
	{
		
		this.canvas = canvas;
		buttonRect = canvas.GetComponentInChildren<UnityEngine.UI.Image>().rectTransform;
		camera = Camera.main;
		canvas.worldCamera = camera;

		Toast.ShowAndroidToastMessage("SetCanvas " + canvas.worldCamera.name);
	}
}
