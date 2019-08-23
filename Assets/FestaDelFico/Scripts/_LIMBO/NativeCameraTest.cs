using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NativeCameraTest : MonoBehaviour
{
	public RawImage photo;

    void Start()
	{
		NativeCamera.TakePicture(CameraCallback);
	}

	void CameraCallback (string path)
	{
		Toast.ShowAndroidToastMessage("Picture taken. Saved at: " + path);
		Debug.Log("###############################################");
		Debug.Log("Picture Taken. Saved at: " + path);
		Debug.Log("###############################################");
	}
}
