using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	public float speed = 2;
    new Camera camera;

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
