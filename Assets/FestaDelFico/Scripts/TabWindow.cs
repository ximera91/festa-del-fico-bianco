using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TabWindow : MonoBehaviour
{
    public GameObject[] tabs;
	public Button[] buttons;
	public GameObject[] decorators;
	public UnityEvent onSwitchTab;

	private int currentTab;
	private int nTab;

	void Awake()
	{
		if(tabs.Length != buttons.Length)
		{
			Debug.LogError("Tabs and buttons number mismatch.");
		}
		if(tabs.Length != decorators.Length)
		{
			Debug.LogWarning("Tabs and decorators number mismatch.");
		}
		if(buttons.Length != decorators.Length)
		{
			Debug.LogWarning("Buttons and decorators number mismatch.");
		}
	}

	void Start()
	{
		nTab = tabs.Length;

		for(int i = 0; i < nTab; i++)
		{
			int tab = i;
			buttons[i].onClick.AddListener(() => SwitchTab(tab));
		}
	}

	public void OnCloseWindow()
	{
		SwitchTab(0);
	}

	private void SwitchTab(int newTab)
	{		
		OpenTab(currentTab, false);
		OpenTab(newTab, true);

		onSwitchTab.Invoke();
	}

	private void OpenTab(int tab, bool open = true)
	{
		tabs[tab].gameObject.SetActive(open);
		buttons[tab].interactable = !open;

		if(decorators.Length > 0)
		{
			if(decorators[tab] != null)
			{
				decorators[tab].SetActive(open);
			}
		}

		if(open)
		{
			currentTab = tab;
		}
	}
}
