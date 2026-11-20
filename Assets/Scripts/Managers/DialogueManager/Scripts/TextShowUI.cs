using System;
using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorForUnity;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(TypewriterComponent))]
[RequireComponent(typeof(TextAnimator))]
[RequireComponent(typeof(CanvasGroup))]
public class TextShowUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private TypewriterComponent _typewriter;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[Header("Behaviour")]
	[InfoBox("Only the hidden typewriter reports Ink line completion to the dialogue manager.")]
	[SerializeField]
	private bool _isPrimary;

	[SerializeField, Min(0f)]
	[Tooltip("Seconds a standalone line stays visible after this typewriter finishes.")]
	private float _standaloneDismissDelay = 3f;

	[HideIf(nameof(_isPrimary))]
	[SerializeField, Min(0f)]
	[Tooltip("Seconds to fade out standalone text after its reading delay.")]
	private float _standaloneFadeDuration = 0.25f;

	private DialogueManager _dialogueManager;
	private DialogueState _state;
	private bool _isStandaloneLine;
	private Tween _dismissTween;

	public event Action StandaloneTextShown;
	public event Action StandaloneLineFinished;

	private void Reset()
	{
		_typewriter = GetComponent<TypewriterComponent>();
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	private void OnEnable()
	{
		_dialogueManager = DialogueManager.Instance;
		if (_dialogueManager)
		{
			_state = _dialogueManager.DialogueState;

			_state.OnDisplayDialogue += ShowLine;
			_state.OnTypewriterSkip += _typewriter.SkipTypewriter;
			_dialogueManager.OnStandaloneContinue += ContinueStandaloneLine;
			_dialogueManager.OnDialogueStarted += HandleDialogueStarted;
			_typewriter.onTextShowed.AddListener(HandleTextShowed);
		}
	}

	private void OnDisable()
	{
		_dismissTween.Stop();
		if (_dialogueManager)
		{
			_state.OnDisplayDialogue -= ShowLine;
			_state.OnTypewriterSkip -= _typewriter.SkipTypewriter;
			_dialogueManager.OnStandaloneContinue -= ContinueStandaloneLine;
			_dialogueManager.OnDialogueStarted -= HandleDialogueStarted;
			_typewriter.onTextShowed.RemoveListener(HandleTextShowed);
			FinishStandaloneLine();
		}
	}

	private void ShowLine(string speaker, string line)
	{
		_canvasGroup.alpha = 1f;
		_typewriter.ShowText(line);
	}

	private void HandleDialogueStarted(string knotName)
	{
		_dismissTween.Stop();
		_isStandaloneLine = knotName == null;
	}

	private void ContinueStandaloneLine()
	{
		if (!_isStandaloneLine)
		{
			return;
		}

		if (_typewriter.IsShowingText)
		{
			_typewriter.SkipTypewriter();
			return;
		}

		FinishStandaloneLine();
	}

	private void HandleTextShowed()
	{
		if (_isStandaloneLine)
		{
			_dismissTween = _isPrimary
				? Tween.Delay(this, _standaloneDismissDelay)
				: Tween.Alpha(
					_canvasGroup,
					0f,
					_standaloneFadeDuration,
					Ease.Linear,
					startDelay: _standaloneDismissDelay
				);
			_dismissTween.OnComplete(this, view => view.FinishStandaloneLine());
			_dismissTween.isPaused = !_state.CanAdvanceStory;
			StandaloneTextShown?.Invoke();
		}
		if (_isPrimary && !_isStandaloneLine)
		{
			_state.TypewriterFinished();
		}
	}

	private void FinishStandaloneLine()
	{
		if (!_isStandaloneLine)
		{
			return;
		}

		_isStandaloneLine = false;
		_dismissTween.Stop();
		_typewriter.StopShowingText();
		_typewriter.TextAnimator.SetText(string.Empty);
		StandaloneLineFinished?.Invoke();
	}
}
