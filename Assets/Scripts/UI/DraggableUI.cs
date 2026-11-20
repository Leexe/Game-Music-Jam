using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

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
	[SerializeField]
	private float _dragOpacity = 0.6f;

	[SerializeField]
	private float _shakeRotation = 10f;

	[SerializeField]
	private float _shakeDuration = 0.3f;

	[HideInInspector]
	public InventorySlotUI InventorySlot;
	public bool IsInteractable { get; private set; } = true;

	private Canvas _rootCanvas;
	private Sequence _shakeTween;

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
		ShakeItem();
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
		ShakeItem();
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
			DialogueManager.Instance.TryShowLine(_itemData.ClickDescription);
		}
	}

	private void ShakeItem()
	{
		_shakeTween.Stop();
		_shakeTween = Sequence
			.Create()
			.Chain(Tween.PunchLocalRotation(transform, new Vector3(0, 0, _shakeRotation), _shakeDuration));
	}
}
