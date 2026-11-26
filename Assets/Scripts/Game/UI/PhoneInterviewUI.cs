using System;
using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PhoneInterviewUI : MonoBehaviour
{
	[Title("References")]
	[SerializeField]
	private Button _phoneButton;

	[SerializeField]
	private NotebookUI _notebook;

	[SerializeField]
	private CanvasGroup _phone;

	[SerializeField]
	private CanvasGroup[] _fadeCanvasGroups = Array.Empty<CanvasGroup>();

	[Title("Fade")]
	[SerializeField, Min(0f)]
	private float _fadeDuration = 0.3f;

	[SerializeField]
	private Ease _fadeEase = Ease.OutQuad;

	[Title("Phone Animation")]
	[SerializeField, Min(0f)]
	private float _phoneStartYOffset = 250f;

	[SerializeField, Min(0f)]
	private float _phoneMoveDuration = 0.35f;

	[SerializeField]
	private Ease _phoneMoveEase = Ease.OutCubic;

	[SerializeField]
	private bool _useUnscaledTime = true;

	private readonly List<RaycastResult> _raycastHits = new();
	private DialogueManager _dialogue;
	private RectTransform _phoneRect;
	private Vector2 _phoneShownPos;
	private Sequence _phoneSequence;
	private bool _canContinue;

	private Vector2 PhoneHiddenPos => _phoneShownPos - new Vector2(0f, _phoneStartYOffset);

	private void Awake()
	{
		_phoneRect = (RectTransform)_phone.transform;
		_phoneShownPos = _phoneRect.anchoredPosition;

		_phone.alpha = 0f;
		foreach (CanvasGroup group in _fadeCanvasGroups)
		{
			group.alpha = 0f;
		}
		_phoneRect.anchoredPosition = PhoneHiddenPos;
	}

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
		_phoneSequence.Stop();

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
		_notebook.gameObject.SetActive(true);
		_notebook.MoveToInterviewPosition();

		_phoneRect.anchoredPosition = PhoneHiddenPos;
		AnimatePhoneUI(1f, _phoneShownPos);

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
		AnimatePhoneUI(0f, PhoneHiddenPos);
	}

	private void AnimatePhoneUI(float targetAlpha, Vector2 targetPos)
	{
		_phoneSequence.Stop();
		_phoneSequence = Sequence
			.Create(useUnscaledTime: _useUnscaledTime)
			.Group(Tween.Alpha(_phone, targetAlpha, _fadeDuration, _fadeEase, useUnscaledTime: _useUnscaledTime))
			.Group(
				Tween.UIAnchoredPosition(
					_phoneRect,
					targetPos,
					_phoneMoveDuration,
					_phoneMoveEase,
					useUnscaledTime: _useUnscaledTime
				)
			);

		foreach (CanvasGroup group in _fadeCanvasGroups)
		{
			_phoneSequence.Group(
				Tween.Alpha(group, targetAlpha, _fadeDuration, _fadeEase, useUnscaledTime: _useUnscaledTime)
			);
		}
	}

	private bool IsPointerOverInteractiveUI(Vector2 screenPos)
	{
		_raycastHits.Clear();
		EventSystem.current.RaycastAll(
			new PointerEventData(EventSystem.current) { position = screenPos },
			_raycastHits
		);

		foreach (RaycastResult hit in _raycastHits)
		{
			if (
				hit.gameObject.transform.IsChildOf(_notebook.transform)
				|| hit.gameObject.GetComponentInParent<Selectable>()
			)
			{
				return true;
			}
		}

		return false;
	}
}
