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

	[SerializeField]
	private ItemData _itemData;

	[Header("Default Values")]
	private float _dragOpacity = 0.6f;

	private Canvas _rootCanvas;

	public bool IsInteractable { get; private set; } = true;

	[HideInInspector]
	public InventorySlotUI InventorySlot;

	private void Reset()
	{
		_rectTransform = GetComponent<RectTransform>();
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	private void OnEnable()
	{
		CacheRootCanvas();
	}

	private void OnCanvasHierarchyChanged()
	{
		CacheRootCanvas();
	}

	private void CacheRootCanvas()
	{
		if (_rootCanvas && _rootCanvas.isRootCanvas && transform.IsChildOf(_rootCanvas.transform))
		{
			return;
		}

		Canvas canvas = GetComponentInParent<Canvas>();
		_rootCanvas = canvas ? canvas.rootCanvas : null;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!IsInteractable)
		{
			return;
		}

		if (InventorySlot && _rootCanvas)
		{
			_rectTransform.SetParent(_rootCanvas.transform, worldPositionStays: true);
			_rectTransform.SetAsLastSibling();
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
