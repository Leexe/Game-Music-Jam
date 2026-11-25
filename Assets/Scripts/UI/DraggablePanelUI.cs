using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanelUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	[Header("References")]
	[SerializeField]
	private RectTransform _target;

	[Tooltip("Optional child point to hold by")]
	[SerializeField]
	private RectTransform _grabPoint;

	[Tooltip("Optional bounds to clamp the drag hitbox inside. Uses _target's parent if null.")]
	[SerializeField]
	private RectTransform _bounds;

	[Header("Inertia")]
	[SerializeField, Min(1f)]
	private float _stiffness = 300f;

	[SerializeField, Min(0.1f)]
	private float _damping = 24f;

	[SerializeField, Min(0.1f)]
	private float _releaseDrag = 14f;

	private bool _isDragging;
	private Vector2 _pointerScreenPos;
	private Camera _eventCamera;
	private Vector2 _velocity;
	private Vector2 _clickOffset;
	private readonly Vector3[] _corners = new Vector3[4];

	private void Reset()
	{
		_target = GetComponent<RectTransform>();
	}

	private void OnDisable()
	{
		_isDragging = false;
		_velocity = Vector2.zero;
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

		if (!_grabPoint)
		{
			RectTransform parent = (RectTransform)_target.parent;
			Vector2 mouseLocal = ScreenToParentLocal(parent, _pointerScreenPos, _eventCamera);
			_clickOffset = mouseLocal - (Vector2)parent.InverseTransformPoint(_target.position);
		}
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
		float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);

		if (_isDragging)
		{
			RectTransform parent = (RectTransform)_target.parent;
			Vector2 mouseLocal = ScreenToParentLocal(parent, _pointerScreenPos, _eventCamera);
			Vector2 holdLocal = _grabPoint
				? (Vector2)parent.InverseTransformPoint(_grabPoint.position)
				: (Vector2)parent.InverseTransformPoint(_target.position) + _clickOffset;
			Vector2 offset = mouseLocal - holdLocal;

			_velocity += offset * (_stiffness * dt);
			_velocity *= Mathf.Exp(-_damping * dt);
		}
		else
		{
			if (_velocity.sqrMagnitude < 0.01f)
			{
				_velocity = Vector2.zero;
				return;
			}

			_velocity *= Mathf.Exp(-_releaseDrag * dt);
		}

		_target.anchoredPosition += _velocity * dt;
	}

	private static Vector2 ScreenToParentLocal(RectTransform parent, Vector2 screenPos, Camera eventCamera)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, eventCamera, out Vector2 local);
		return local;
	}
}
