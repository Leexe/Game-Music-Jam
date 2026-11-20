using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	[Header("References")]
	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[Header("Default Values")]
	private float _dragOpacity = 0.6f;

	public bool IsInteractable { get; private set; } = true;

	[HideInInspector]
	public InventorySlotUI InventorySlot;

	private void Reset()
	{
		_rectTransform = GetComponent<RectTransform>();
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!IsInteractable)
		{
			return;
		}

		_canvasGroup.alpha = _dragOpacity;
		_canvasGroup.blocksRaycasts = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!IsInteractable)
		{
			return;
		}

		_rectTransform.anchoredPosition += eventData.delta;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (InventorySlot)
		{
			InventorySlot.RealignItem();
		}

		_canvasGroup.alpha = 1f;
		_canvasGroup.blocksRaycasts = true;
	}

	public void SetInventorySlot(InventorySlotUI inventorySlot)
	{
		InventorySlot = inventorySlot;
	}

	public void RemoveFromInventorySlot()
	{
		InventorySlot = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (IsInteractable && eventData.button == PointerEventData.InputButton.Left)
		{
			DialogueManager.Instance.TryShowLine(gameObject.name);
		}
	}
}
