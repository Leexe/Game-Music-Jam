using System;
using PrimeTween;
using UnityEngine;

/// <summary>
/// Point-and-click room switching. Shows one room at a time and hides the rest.
/// </summary>
public class RoomManager : MonoSingleton<RoomManager>
{
	[SerializeField]
	private GameObject[] _rooms;

	[SerializeField]
	private GameObject _startRoom;

	[Header("Transition")]
	[SerializeField]
	private float _blackHoldDuration = 0.5f;

	[SerializeField]
	private CanvasGroup _fade;

	[SerializeField]
	private float _zoomScale = 2.5f;

	[SerializeField]
	private float _zoomDuration = 0.6f;

	[SerializeField]
	private float _fadeInDuration = 0.4f;

	// Events
	[HideInInspector]
	public Action<GameObject> OnRoomChanged;

	public GameObject CurrentRoom { get; private set; }

	public bool IsTransitioning { get; private set; }

	private void Start()
	{
		_fade.alpha = 0f;
		_fade.blocksRaycasts = false;

		// If the start room was deleted, fall back to the first room that still exists
		GameObject start = _startRoom != null ? _startRoom : Array.Find(_rooms, r => r != null);
		GoTo(start);
	}

	/// <summary>
	/// Zooms into the door, fades to black, swaps rooms, then fades back in
	/// </summary>
	/// <param name="room">Room To Enter</param>
	/// <param name="door">The door that was clicked (zoom target)</param>
	public void EnterRoom(GameObject room, RectTransform door)
	{
		if (IsTransitioning || room == null || room == CurrentRoom)
		{
			return;
		}

		IsTransitioning = true;
		_fade.blocksRaycasts = true; // stops extra clicks mid-transition

		RectTransform roomRect = CurrentRoom.GetComponent<RectTransform>();
		Vector2 startPos = roomRect.anchoredPosition;

		// Where the door is inside the room, so we can zoom toward it
		Vector2 doorPoint = roomRect.InverseTransformPoint(door.position);
		Vector2 endPos = startPos - doorPoint * _zoomScale;
		// Don't slide so far that the room's edges come into view
		Vector2 maxOffset = roomRect.rect.size * (_zoomScale - 1f) / 2f;
		Vector2 offset = endPos - startPos;
		offset.x = Mathf.Clamp(offset.x, -maxOffset.x, maxOffset.x);
		offset.y = Mathf.Clamp(offset.y, -maxOffset.y, maxOffset.y);
		endPos = startPos + offset;

		Sequence
			.Create()
			.Group(Tween.Scale(roomRect, _zoomScale, _zoomDuration, Ease.InQuad))
			.Group(Tween.UIAnchoredPosition(roomRect, endPos, _zoomDuration, Ease.InQuad))
			.Group(Tween.Alpha(_fade, 1f, _zoomDuration, Ease.InQuad))
			.ChainCallback(() =>
			{
				// Reset the old room so it looks normal next time we come back
				roomRect.localScale = Vector3.one;
				roomRect.anchoredPosition = startPos;
				GoTo(room);
			})
			.ChainDelay(_blackHoldDuration) // stay black for a moment
			.Chain(Tween.Alpha(_fade, 0f, _fadeInDuration, Ease.OutQuad))
			.ChainCallback(() =>
			{
				_fade.blocksRaycasts = false;
				IsTransitioning = false;
			});
	}

	/// <summary>
	/// Instantly shows the chosen room and hides every other room
	/// </summary>
	/// <param name="room">Room To Show</param>
	public void GoTo(GameObject room)
	{
		if (room == null || room == CurrentRoom)
		{
			return;
		}

		foreach (GameObject r in _rooms)
		{
			if (r == null)
			{
				continue;
			}

			r.SetActive(r == room);
		}

		CurrentRoom = room;
		OnRoomChanged?.Invoke(room);
	}
}
