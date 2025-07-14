using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextButton : Button
{
	private TMP_Text text;

	protected override void Awake()
	{
		base.Awake();
		text = targetGraphic.GetComponent<TMP_Text>();
		if(text == null)
		{
			Debug.LogError("TMP_Text component not found in children of button " + name);
		}
	}

	protected override void OnEnable()
	{
		text = targetGraphic.GetComponent<TMP_Text>();
		if(text == null)
		{
			Debug.LogError("TMP_Text component not found in children of button " + name);
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		switch (state)
		{
			case SelectionState.Disabled:		
			text.fontStyle = FontStyles.Normal;
			text.color = this.colors.disabledColor;
			break;
			case SelectionState.Pressed:
			text.fontStyle = FontStyles.Underline;
			text.color = this.colors.pressedColor;
			break;
			case SelectionState.Normal:
			text.fontStyle = FontStyles.Normal;
			text.color = this.colors.normalColor;
			break;
			case SelectionState.Highlighted:			
			text.fontStyle = FontStyles.Normal;
			text.color = this.colors.highlightedColor;
			break;
			case SelectionState.Selected:			
			text.fontStyle = FontStyles.Normal;
			text.color = this.colors.selectedColor;
			break;
		}
		
	}
}
