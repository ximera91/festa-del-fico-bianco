using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationManager : MonoBehaviour
{
	public string toastMessage = "";
	public int sceneToGoBackTo;
	private bool escapePressed;
	private float escapePressedTime = 0;
	private float escapeTimeout = 2;
	private bool semaphore = false; 

    void Update()
	{
		if(Input.GetKeyUp(KeyCode.Escape))
		{
			if(!escapePressed)
			{
				Toast.ShowAndroidToastMessage(toastMessage);
				escapePressed = true;
				escapePressedTime = Time.realtimeSinceStartup;
			}
			else
			{
				if(Time.realtimeSinceStartup - escapePressedTime < escapeTimeout)
				{
					if(sceneToGoBackTo < 0)
					{
						Application.Quit();
					}
					else
					{
						SceneManager.LoadScene(sceneToGoBackTo, LoadSceneMode.Single);
					}
				}
				else
				{
					escapePressed = false;
					escapePressedTime = 0;
				}
			}
		}

		if(escapePressed && Time.realtimeSinceStartup - escapePressedTime > escapeTimeout)
		{
			escapePressed = false;
			escapePressedTime = 0;
		}
	}

	public void OpenScene(int sceneIndex)
	{
		SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
	}

	public void OpenScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
	}
}
