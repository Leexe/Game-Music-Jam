using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanelUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	[SerializeField]
	private RectTransform _target;

	[Tooltip("Optional child point to hold by")]
	[SerializeField]
	private RectTransform _grabPoint;

	[SerializeField, Min(0.1f)]
	private float _followSpeed = 20f;

	private bool _isDragging;
	private Vector2 _pointerScreenPos;
	private Camera _eventCamera;

	private void Reset()
	{
		_target = GetComponent<RectTransform>();
	}

	private void OnDisable()
	{
		_isDragging = false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
		{
			return;
		}

		_isDragging = true;
		_pointerScreenPos = eventData.position;
		_eventCamera = eventData.pressEventCamera;
		_target.SetAsLastSibling();
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			_pointerScreenPos = eventData.position;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			_isDragging = false;
		}
	}

	private void Update()
	{
		if (!_isDragging)
		{
			return;
		}

		RectTransform parent = (RectTransform)_target.parent;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			parent,
			_pointerScreenPos,
			_eventCamera,
			out Vector2 mouseLocal
		);

		RectTransform handle = _grabPoint ? _grabPoint : _target;
		Vector2 handleLocal = parent.InverseTransformPoint(handle.position);
		Vector2 desiredAnchoredPos = _target.anchoredPosition + (mouseLocal - handleLocal);

		float t = 1f - Mathf.Exp(-_followSpeed * Time.unscaledDeltaTime);
		_target.anchoredPosition = Vector2.Lerp(_target.anchoredPosition, desiredAnchoredPos, t);
	}
}
