using System;
using PrimeTween;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
	private static float _effectTimeScale = 1f; // temp effects

	public float RunTime;
	private Sequence _timeSlowSequence;

	[HideInInspector]
	public Action<bool> OnInteractableEnter;

	[HideInInspector]
	public Action OnInteractableExit;

	[HideInInspector]
	public Action<string> OnItemPickUp;

	public static bool IsPaused { get; private set; }

	// Unity Functions

	private void Start()
	{
		PrimeTweenConfig.warnZeroDuration = false;
	}

	// Event Triggers

	public void TriggerOnInteractableEnter(bool isAfterDialogue = false)
	{
		OnInteractableEnter?.Invoke(isAfterDialogue);
	}

	public void TriggerOnInteractableExit()
	{
		OnInteractableExit?.Invoke();
	}

	public void TriggerOnItemPickUp(string itemId)
	{
		OnItemPickUp?.Invoke(itemId);
	}

	// Cursor

	public static void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public static void UnlockCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}
}
