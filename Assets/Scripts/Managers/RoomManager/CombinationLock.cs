using System;
using TMPro;
using UnityEngine;

public class CombinationLock : MonoBehaviour
{
	[SerializeField] private string _combination = "1234";
	[SerializeField] private GameObject _panel;          // the lock UI, hidden at start
	[SerializeField] private TMP_InputField _input;      // where the player types the code
	[SerializeField] private bool _enterRoomOnUnlock = true;

	public bool IsLocked { get; private set; } = true;
	public Action OnUnlocked;

	private Door _pendingDoor;

	private void Awake() => _panel.SetActive(false);

	// Called by Door when the player clicks a locked door
	public void Open(Door door)
	{
		_pendingDoor = door;
		_input.text = "";
		_panel.SetActive(true);
	}

	// Hook up to the panel's "Enter" button
	public void Submit()
	{
		if (_input.text != _combination)
		{
			_input.text = "";   // wrong code: clear it (add a buzz SFX here)
			return;
		}

		IsLocked = false;
		_panel.SetActive(false);
		OnUnlocked?.Invoke();
		if (_enterRoomOnUnlock && _pendingDoor != null) _pendingDoor.Enter();
	}

	// Hook up to the panel's "X" / back button
	public void Close() => _panel.SetActive(false);

	public void Lock() => IsLocked = true;   // relock from anywhere if needed
}
