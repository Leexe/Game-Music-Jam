using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	[Header("References")]
	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private RectTransform _imageRectTransform;

	[Tooltip("Optional child point to hold by. Uses _rectTransform if null.")]
	[SerializeField]
	private RectTransform _grabPoint;

	[SerializeField]
	private TextMeshProUGUI _itemName;

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

	[SerializeField, Min(0.1f)]
	private float _followSpeed = 20f;

	[HideInInspector]
	public InventorySlotUI InventorySlot;
	public bool IsInteractable { get; private set; } = true;
	public ItemData ItemData => _itemData;

	private Canvas _rootCanvas;
	private Sequence _shakeTween;
	private bool _isDragging;
	private Vector2 _pointerScreenPos;
	private Camera _eventCamera;

	private void Reset()
	{
		_rectTransform = GetComponent<RectTransform>();
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	private void OnEnable()
	{
		CacheRootCanvas();
		if (_itemData)
		{
			_itemName.text = _itemData.name;
		}
	}

	private void OnDisable()
	{
		_isDragging = false;
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

		_isDragging = true;
		_pointerScreenPos = eventData.position;
		_eventCamera = eventData.pressEventCamera;
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

		_pointerScreenPos = eventData.position;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_isDragging = false;
		if (InventorySlot)
		{
			InventorySlot.RealignItem();
		}

		_canvasGroup.alpha = 1f;
		_canvasGroup.blocksRaycasts = true;
	}

	private void Update()
	{
		if (!_isDragging)
		{
			return;
		}

		var parent = (RectTransform)_rectTransform.parent;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			parent,
			_pointerScreenPos,
			_eventCamera,
			out Vector2 mouseLocal
		);

		RectTransform handle = _grabPoint ? _grabPoint : _rectTransform;
		Vector2 handleLocal = parent.InverseTransformPoint(handle.position);
		Vector2 desiredAnchoredPos = _rectTransform.anchoredPosition + (mouseLocal - handleLocal);

		float t = 1f - Mathf.Exp(-_followSpeed * Time.unscaledDeltaTime);
		_rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, desiredAnchoredPos, t);
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

	public void SetItemData(ItemData itemData)
	{
		_itemData = itemData;
		if (_itemName && _itemData)
		{
			_itemName.text = _itemData.name;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (IsInteractable && eventData.button == PointerEventData.InputButton.Left && _itemData)
		{
			DialogueManager.Instance.DisplayLoneLine(_itemData.ClickDescription);
		}
	}

	private void ShakeItem()
	{
		_shakeTween.Stop();
		_shakeTween = Sequence
			.Create()
			.Chain(Tween.PunchLocalRotation(_imageRectTransform, new Vector3(0, 0, _shakeRotation), _shakeDuration));
	}

	private void ShowText()
	{
		_itemName.text = _itemData.name;
	}

	private void HideText() { }
}
