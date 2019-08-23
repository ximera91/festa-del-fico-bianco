using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Sticker : Graphic
{
	[Range(0.1f, 2f)]
	public float minScale = 0.75f;
	[Range(2f, 5f)]
	public float maxScale = 3f;
	public float deleteButtonYOffset = 100f;

	[SerializeField]
	private Image image = null;
	[SerializeField]
	private Button deleteButton = null;
	[SerializeField]
	private Graphic selectedGraphic = null;

	private bool selected = false;
	private Vector2 nextPosition;
	private float smoothTime = 0.2f;
	private float maxSpeed = 200;
	private Vector2 currentVelocity;
	private bool smooth;
	private bool normalizeScale = false;
	private Vector3 normalizedScale;
	private float buttonYOffset;

	public void AddListenerDeleteButton(UnityAction call)
	{
		deleteButton.onClick.AddListener(call);
	}

	void Update()
	{
		if(smooth)
		{
			rectTransform.anchoredPosition = 
				Vector2.SmoothDamp(
					rectTransform.anchoredPosition,
					nextPosition,
					ref currentVelocity,
					smoothTime,
					maxSpeed);
		}
		else
		{
			rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, nextPosition, 1);
		}

		if(normalizeScale)
		{
			rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, normalizedScale, 0.2f);
		}

		if(selected)
		{
			float scale = rectTransform.localScale.x;
			deleteButton.targetGraphic.rectTransform.rotation = Quaternion.identity;
			deleteButton.targetGraphic.rectTransform.position = rectTransform.position + Vector3.down * buttonYOffset * scale;
			deleteButton.targetGraphic.rectTransform.localScale = Vector3.one * 1 / scale;
		}
	}

	public void SetSprite(Sprite sprite)
	{
		image.sprite = sprite;
		rectTransform.sizeDelta = sprite.bounds.size;
		buttonYOffset = deleteButtonYOffset  * (Screen.height / 2340) + sprite.bounds.extents.y; 
	}

	public bool IsSelected()
	{
		return selected;
	}

	[ContextMenu("Select")]
	public void Select()
	{
		selected = true;
		selectedGraphic.gameObject.SetActive(true);
		deleteButton.gameObject.SetActive(true);
	}

	public void Deselect()
	{
		selected = false;
		selectedGraphic.gameObject.SetActive(false);
		deleteButton.gameObject.SetActive(false);
	}

    public void MoveTo(Vector2 position, bool smooth = false)
	{
		nextPosition = position;
		this.smooth = smooth;
	}

	public void Rotate(Quaternion rotation)
	{
		rectTransform.localRotation = rotation;
	}

	public void Scale(float scale)
	{
		rectTransform.localScale = Vector3.one * scale;
		normalizeScale = false;
	}

	public void ScaleMultiplier(float multiplier)
	{
		rectTransform.localScale *= multiplier;
		normalizeScale = false;
	}

	public void NormalizeScale()
	{
		float currentScale = rectTransform.localScale.x;
		if(currentScale > maxScale * maxScale)
		{
			normalizeScale = true;
			normalizedScale = Vector3.one * maxScale;
			return;
		}
		else if (currentScale < minScale * minScale)
		{
			normalizeScale = true;
			normalizedScale = Vector3.one * minScale;
			return;
		}
	}
}
