using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PhoneInterviewUI : MonoBehaviour
{
	[SerializeField]
	private Button _phoneButton;

	[SerializeField]
	private GameObject _notebook;

	private readonly List<RaycastResult> _raycastHits = new();
	private DialogueManager _dialogue;
	private bool _canContinue;

	private void OnEnable()
	{
		_dialogue = DialogueManager.Instance;
		_phoneButton.onClick.AddListener(StartInterview);
		_dialogue.DialogueState.OnStartDialogue += HandleDialogueStart;
		_dialogue.DialogueState.OnDisplayChoices += HandleChoicesDisplayed;
		_dialogue.OnDialogueEnded += EndInterview;
	}

	private void Update()
	{
		if (
			!_canContinue
			|| !_dialogue.DialogueState.CanAdvanceStory
			|| Mouse.current == null
			|| !Mouse.current.leftButton.wasPressedThisFrame
			|| IsPointerOverInteractiveUI(Mouse.current.position.ReadValue())
		)
		{
			return;
		}

		ContinueInterview();
	}

	private void OnDisable()
	{
		_canContinue = false;
		_phoneButton.onClick.RemoveListener(StartInterview);
		if (_dialogue)
		{
			_dialogue.DialogueState.OnStartDialogue -= HandleDialogueStart;
			_dialogue.DialogueState.OnDisplayChoices -= HandleChoicesDisplayed;
			_dialogue.OnDialogueEnded -= EndInterview;
		}
	}

	private void StartInterview()
	{
		if (_dialogue.IsStoryPlaying || !_dialogue.DialogueState.CanAdvanceStory)
		{
			return;
		}

		EventSystem.current.SetSelectedGameObject(null);
		_notebook.SetActive(false);
		_dialogue.DialogueState.StartStory("new_interview");
		_phoneButton.interactable = !_dialogue.IsStoryPlaying;
	}

	private void ContinueInterview()
	{
		EventSystem.current.SetSelectedGameObject(null);
		_dialogue.ContinueStory();
	}

	private void HandleDialogueStart() => _canContinue = _dialogue.IsStoryPlaying;

	private void HandleChoicesDisplayed(List<string> choices) => _canContinue = false;

	private void EndInterview()
	{
		_canContinue = false;
		_phoneButton.interactable = true;
	}

	private bool IsPointerOverInteractiveUI(Vector2 screenPos)
	{
		_raycastHits.Clear();
		EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPos }, _raycastHits);

		foreach (RaycastResult hit in _raycastHits)
		{
			if (hit.gameObject.transform.IsChildOf(_notebook.transform) || hit.gameObject.GetComponentInParent<Selectable>())
			{
				return true;
			}
		}

		return false;
	}
}
