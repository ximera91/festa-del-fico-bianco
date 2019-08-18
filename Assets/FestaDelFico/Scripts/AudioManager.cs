using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Audio Manager")]
public class AudioManager : ScriptableObject
{
    public AudioClip cameraShutter;

	public void PlayCameraShutter()
	{
		AudioSource.PlayClipAtPoint(cameraShutter, Camera.main.transform.position);
	}
}
