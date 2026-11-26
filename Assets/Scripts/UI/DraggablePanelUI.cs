using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanelUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	private const float MaxDeltaTime = 0.05f;
	private const float StopVelocityThresholdSqr = 0.01f;

	[Title("References")]
	[SerializeField]
	private RectTransform _target;

	[Tooltip("Optional child point to hold by")]
	[SerializeField]
	private RectTransform _grabPoint;

	[Tooltip("Optional bounds to clamp the drag hitbox inside. Uses _target's parent if null.")]
	[SerializeField]
	private RectTransform _bounds;

	[Title("Behavior")]
	[Tooltip("Moves the target to the top of the UI draw order when grabbed.")]
	[SerializeField]
	private bool _bringToFront = true;

	[Title("Inertia")]
	[SerializeField, Min(1f)]
	private float _stiffness = 300f;

	[SerializeField, Min(0.1f)]
	private float _damping = 24f;

	[SerializeField, Min(0.1f)]
	private float _releaseDrag = 14f;

	[Tooltip("How much velocity is reflected when hitting a boundary (0 = stop, 1 = full bounce).")]
	[SerializeField, Range(0f, 1f)]
	private float _boundaryBounce = 0.4f;

	private bool _isDragging;
	private Tween _dockTween;
	private Vector2 _pointerScreenPos;
	private Camera _eventCamera;
	private Vector2 _velocity;
	private Vector2 _clickOffset;
	private readonly Vector3[] _corners = new Vector3[4];

	private RectTransform TargetParent => (RectTransform)_target.parent;

	#region Unity Lifecycle

	private void Reset()
	{
		_target = GetComponent<RectTransform>();
	}

	private void OnDisable()
	{
		StopDocking();
		_isDragging = false;
		_velocity = Vector2.zero;
	}

	private void Update()
	{
		float dt = Mathf.Min(Time.unscaledDeltaTime, MaxDeltaTime);

		if (_isDragging)
		{
			UpdateDragVelocity(dt);
		}
		else if (!ApplyReleaseDrag(dt))
		{
			return;
		}

		_target.anchoredPosition += _velocity * dt;
		ClampToBounds();
	}

	#endregion

	#region Pointer Events

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
		{
			return;
		}

		StopDocking();
		_isDragging = true;
		_pointerScreenPos = eventData.position;
		_eventCamera = eventData.pressEventCamera;

		if (_bringToFront)
		{
			_target.SetAsLastSibling();
		}

		if (!_grabPoint)
		{
			Vector2 mouseLocal = ScreenToParentLocal(_pointerScreenPos, _eventCamera);
			_clickOffset = mouseLocal - (Vector2)TargetParent.InverseTransformPoint(_target.position);
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

	#endregion

	#region Inertia & Clamping

	private Vector2 GetDockOffset(RectTransform bar)
	{
		GetHitboxLocalBounds(bar, out Vector2 handleMin, out _);
		Vector2 dockCorner = new(bar.rect.xMin, bar.rect.yMax);
		return dockCorner - handleMin;
	}

	public bool IsDockedAbove(RectTransform bar)
	{
		const float tolerance = 1f;
		return GetDockOffset(bar).sqrMagnitude <= tolerance * tolerance;
	}

	public void PlaceAboveBar(RectTransform bar)
	{
		Vector3 destination = _target.position + bar.TransformVector(GetDockOffset(bar));
		MoveToPosition(destination, 0.35f, Ease.OutCubic, true);
		_dockTween.OnComplete(this, static panel => panel.ClampToBounds());
	}

	public void MoveToPosition(Vector3 destination, float duration, Ease ease, bool useUnscaledTime)
	{
		StopDocking();
		_isDragging = false;
		_velocity = Vector2.zero;
		_dockTween = Tween.Position(_target, destination, duration, ease, useUnscaledTime: useUnscaledTime);
	}

	public void StopDocking()
	{
		_dockTween.Stop();
	}

	private void UpdateDragVelocity(float dt)
	{
		Vector2 mouseLocal = ScreenToParentLocal(_pointerScreenPos, _eventCamera);
		Vector2 holdLocal = _grabPoint
			? (Vector2)TargetParent.InverseTransformPoint(_grabPoint.position)
			: (Vector2)TargetParent.InverseTransformPoint(_target.position) + _clickOffset;

		_velocity += (mouseLocal - holdLocal) * (_stiffness * dt);
		_velocity *= Mathf.Exp(-_damping * dt);
	}

	private bool ApplyReleaseDrag(float dt)
	{
		if (_velocity.sqrMagnitude < StopVelocityThresholdSqr)
		{
			_velocity = Vector2.zero;
			return false;
		}

		_velocity *= Mathf.Exp(-_releaseDrag * dt);
		return true;
	}

	private void ClampToBounds()
	{
		RectTransform boundsRect = _bounds ? _bounds : TargetParent;
		GetHitboxLocalBounds(boundsRect, out Vector2 min, out Vector2 max);

		Rect bounds = boundsRect.rect;
		Vector2 correction = new(
			ComputeAxisCorrection(min.x, max.x, bounds.xMin, bounds.xMax),
			ComputeAxisCorrection(min.y, max.y, bounds.yMin, bounds.yMax)
		);

		if (correction == Vector2.zero)
		{
			return;
		}

		Vector3 worldCorrection = boundsRect.TransformVector(correction);
		_target.anchoredPosition += (Vector2)TargetParent.InverseTransformVector(worldCorrection);
		_velocity.x = ReflectOnImpact(correction.x, _velocity.x);
		_velocity.y = ReflectOnImpact(correction.y, _velocity.y);
	}

	private float ReflectOnImpact(float correction, float velocity)
	{
		if (correction != 0f && velocity * correction < 0f)
		{
			return -velocity * _boundaryBounce;
		}

		return velocity;
	}

	private void GetHitboxLocalBounds(RectTransform boundsRect, out Vector2 min, out Vector2 max)
	{
		((RectTransform)transform).GetWorldCorners(_corners);
		min = Vector2.positiveInfinity;
		max = Vector2.negativeInfinity;

		for (int i = 0; i < 4; i++)
		{
			Vector2 local = boundsRect.InverseTransformPoint(_corners[i]);
			min = Vector2.Min(min, local);
			max = Vector2.Max(max, local);
		}
	}

	private static float ComputeAxisCorrection(float min, float max, float boundMin, float boundMax)
	{
		if (min < boundMin)
		{
			return boundMin - min;
		}
		if (max > boundMax)
		{
			return boundMax - max;
		}
		return 0f;
	}

	private Vector2 ScreenToParentLocal(Vector2 screenPos, Camera eventCamera)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			TargetParent,
			screenPos,
			eventCamera,
			out Vector2 local
		);
		return local;
	}

	#endregion
}
