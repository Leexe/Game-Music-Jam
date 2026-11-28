using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Click the camera to pick it up (it follows the mouse).
/// Click a PhotoSpot to take a photo. Click anywhere else or right-click to put it back.
/// </summary>
public class CameraItem : MonoBehaviour
{
	[Tooltip("Hide the normal mouse cursor while holding the camera.")]
	[SerializeField]
	private bool _hideCursorWhileHeld = true;

	[SerializeField]
	private string _nothingCapturedText = "The camera captured nothing.";

	[SerializeField, Min(0f)]
	private float _nothingCapturedDelay = 1f;

	private RectTransform _rect;
	private Canvas _rootCanvas;

	// Where the camera lives when it's not being held
	private Transform _homeParent;
	private Vector2 _homePosition;
	private int _homeSiblingIndex;

	private readonly List<RaycastResult> _hits = new List<RaycastResult>();
	private Tween _nothingDialogueDelayTween;

	private static CameraItem _active;
	private static bool _suppressClickUntilRelease;

	public static bool ShouldIgnoreClick => _active != null || _suppressClickUntilRelease;

	public bool IsHeld { get; private set; }

	private void Awake()
	{
		_rect = (RectTransform)transform;
		_rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
	}

	private void Update()
	{
		Mouse mouse = Mouse.current;
		if (mouse == null || Time.timeScale == 0f)
		{
			return;
		}

		Vector2 mousePos = mouse.position.ReadValue();

		// Not holding: click on the camera to pick it up
		if (!IsHeld)
		{
			if (mouse.leftButton.wasPressedThisFrame && IsMouseOverCamera(mousePos))
			{
				PickUp();
			}
			return;
		}

		// Holding: follow the mouse
		FollowMouse(mousePos);

		// Right-click = put it back
		if (mouse.rightButton.wasPressedThisFrame)
		{
			PutBack();
			return;
		}

		// Left-click = take a photo if there's a clue under the mouse, then put it back
		if (mouse.leftButton.wasPressedThisFrame)
		{
			_suppressClickUntilRelease = true;
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CameraClick_Sfx);
			_nothingDialogueDelayTween.Stop();
			PhotoSpot spot = FindPhotoSpotUnder(mousePos);
			PutBack();

			if (spot != null)
			{
				CameraManager.Instance.TakePhoto(spot);
			}
			else
			{
				CameraManager.Instance.Flash();
				_nothingDialogueDelayTween = Tween.Delay(
					this,
					_nothingCapturedDelay,
					static camera => DialogueManager.Instance.DisplayLoneLine(camera._nothingCapturedText)
				);
			}
		}
	}

	private void LateUpdate()
	{
		if (_suppressClickUntilRelease && !Mouse.current.leftButton.isPressed)
		{
			_suppressClickUntilRelease = false;
		}
	}

	private void PickUp()
	{
		_homeParent = transform.parent;
		_homePosition = _rect.anchoredPosition;
		_homeSiblingIndex = transform.GetSiblingIndex();

		// Move to the top of the canvas so it draws over everything while held
		transform.SetParent(_rootCanvas.transform, true);
		transform.SetAsLastSibling();

		IsHeld = true;
		_active = this;

		if (_hideCursorWhileHeld)
		{
			Cursor.visible = false;
		}
	}

	private void PutBack()
	{
		transform.SetParent(_homeParent, false);
		transform.SetSiblingIndex(_homeSiblingIndex);
		_rect.anchoredPosition = _homePosition;

		IsHeld = false;
		_active = null;
		Cursor.visible = true;
	}

	private void FollowMouse(Vector2 mousePos)
	{
		RectTransform canvasRect = (RectTransform)_rootCanvas.transform;
		Camera cam = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera;

		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePos, cam, out Vector2 localPos))
		{
			_rect.localPosition = localPos;
		}
	}

	/// True if the camera is the top-most UI thing under the mouse
	private bool IsMouseOverCamera(Vector2 mousePos)
	{
		RaycastUI(mousePos);
		return _hits.Count > 0 && _hits[0].gameObject.transform.IsChildOf(transform);
	}

	/// Finds a PhotoSpot under the mouse (ignoring the camera itself)
	private PhotoSpot FindPhotoSpotUnder(Vector2 mousePos)
	{
		RaycastUI(mousePos);
		foreach (RaycastResult hit in _hits)
		{
			if (hit.gameObject.transform.IsChildOf(transform))
			{
				continue;
			}

			PhotoSpot spot = hit.gameObject.GetComponentInParent<PhotoSpot>();
			if (spot != null && !spot.HasBeenPhotographed && spot.PastImage != null)
			{
				return spot;
			}
		}
		return null;
	}

	private void RaycastUI(Vector2 mousePos)
	{
		_hits.Clear();
		var pointer = new PointerEventData(EventSystem.current) { position = mousePos };
		EventSystem.current.RaycastAll(pointer, _hits);
	}
}
