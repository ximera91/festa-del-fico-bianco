using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.Examples;
using GoogleARCore.Examples.ObjectManipulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FicoController : MonoBehaviour
{
	public LayerMask buttonLayerMask;
	public GameObject button;
	public Material buttonPressedMaterial;
	public Material buttonNormalMaterial;

	private MeshRenderer buttonRenderer;
	private Manipulator manipulator;
	private new Camera camera;

	void Start()
	{
		camera = Camera.main;
		manipulator = GetComponentInParent<SelectionManipulator>();
		buttonRenderer = button.GetComponentInChildren<MeshRenderer>();
	}

	void Update()
	{
		if(manipulator.IsSelected())
		{
			button.SetActive(true);
		}

		if(button.activeInHierarchy)
		{
			if(!manipulator.IsSelected())
			{
				button.SetActive(false);
			}

			button.transform.LookAt(camera.transform);

			if(Input.touchCount == 1)
			{
				Touch t = Input.touches[0];
				RaycastHit hit;
				if(t.phase == TouchPhase.Began)
				{
					if(Physics.Raycast(camera.ScreenPointToRay(t.position), out hit, 20, buttonLayerMask))
					{
						if(hit.transform.tag.Equals(button.tag))
						{
							buttonRenderer.material = buttonPressedMaterial;
						}
					}
				}
				else if(t.phase == TouchPhase.Ended)
				{
					if(Physics.Raycast(camera.ScreenPointToRay(t.position), out hit, 20, buttonLayerMask))
					{
						if(hit.transform.tag.Equals(button.tag))
						{
							buttonRenderer.material = buttonNormalMaterial;
							Remove();
						}
					}
				}
			}
			#if UNITY_EDITOR
			if(Input.GetMouseButtonDown(0))
			{
				Debug.Log("Wella1");
				RaycastHit hit;
				if(Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 20, buttonLayerMask))
				{
					Debug.Log("raycast hit");
					if(hit.transform.tag.Equals(button.tag))
					{
						buttonRenderer.material = buttonPressedMaterial;
					}
				}
			}
			else if(Input.GetMouseButtonUp(0))
			{
				RaycastHit hit;
				if(Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, 20, buttonLayerMask))
				{
					if(hit.transform.tag.Equals(button.tag))
					{
						buttonRenderer.material = buttonNormalMaterial;
						Remove();
					}
				}
			}
			#endif
		}
	}

	[ContextMenu("Set Selected")]
	public void SetSelected()
	{
		ManipulationSystem.Instance.Select(manipulator.gameObject);
	}

	public void Remove()
	{
		ManipulationSystem.Instance.Deselect();
		if(manipulator != null)
		{
			manipulator.enabled = false;
			Destroy(manipulator.gameObject);
		}
	}
}