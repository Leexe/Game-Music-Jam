using System;
using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorForUnity;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(TypewriterComponent))]
[RequireComponent(typeof(TextAnimator))]
[RequireComponent(typeof(CanvasGroup))]
public class TextShowUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private TypewriterComponent _typewriter;

	[SerializeField]
	private CanvasGroup _textCanvasGroup;

	[SerializeField]
	private CanvasGroup _fadeCanvasGroup;

	[Header("Behaviour")]
	[InfoBox("Only the hidden typewriter reports Ink line completion to the dialogue manager.")]
	[FormerlySerializedAs("_isPrimary")]
	[SerializeField]
	private bool _isHidden;

	[SerializeField, Min(0f)]
	[Tooltip("Seconds a standalone line stays visible after this typewriter finishes.")]
	private float _standaloneDismissDelay = 3f;

	[HideIf(nameof(_isHidden))]
	[SerializeField, Min(0f)]
	[Tooltip("Seconds to fade standalone text in and out.")]
	private float _standaloneTextFadeDuration = 0.25f;

	[HideIf(nameof(_isHidden))]
	[SerializeField, Min(0f)]
	[Tooltip("Seconds to fade the standalone background in and out.")]
	private float _standaloneBgFadeDuration = 0.25f;

	private DialogueManager _dialogueManager;
	private DialogueState _state;
	private bool _isStandaloneLine;
	private bool _isFadingOut;
	private Sequence _showSequence;
	private Sequence _dismissSequence;

	public event Action StandaloneTextShown;
	public event Action StandaloneLineFinished;

	private void Reset()
	{
		_typewriter = GetComponent<TypewriterComponent>();
	}

	private void OnEnable()
	{
		_dialogueManager = DialogueManager.Instance;
		if (_dialogueManager)
		{
			_state = _dialogueManager.DialogueState;

			_state.OnDisplayDialogue += ShowLine;
			_state.OnTypewriterSkip += _typewriter.SkipTypewriter;
			_state.OnEndStory += HandleEndStory;
			_dialogueManager.OnStandaloneContinue += ContinueStandaloneLine;
			_dialogueManager.OnDialogueStarted += HandleDialogueStarted;
			_typewriter.onTextShowed.AddListener(HandleTextShowed);
		}

		ClearLine();
	}

	private void OnDisable()
	{
		StopActiveSequences();
		if (_dialogueManager)
		{
			_state.OnDisplayDialogue -= ShowLine;
			_state.OnTypewriterSkip -= _typewriter.SkipTypewriter;
			_state.OnEndStory -= HandleEndStory;
			_dialogueManager.OnStandaloneContinue -= ContinueStandaloneLine;
			_dialogueManager.OnDialogueStarted -= HandleDialogueStarted;
			_typewriter.onTextShowed.RemoveListener(HandleTextShowed);
			FinishStandaloneLine();
		}
	}

	private void ShowLine(string speaker, string line)
	{
		StopActiveSequences();

		if (_isStandaloneLine && !_isHidden)
		{
			StartStandaloneShow(line);
			return;
		}

		SetCanvasGroups(1f, true);
		_typewriter.ShowText(line);
	}

	private void HandleDialogueStarted(string knotName)
	{
		StopActiveSequences();
		_isStandaloneLine = knotName == null;
	}

	private void ContinueStandaloneLine()
	{
		if (!_isStandaloneLine || _isFadingOut)
		{
			return;
		}

		_showSequence.Complete();

		if (_typewriter.IsShowingText)
		{
			_typewriter.SkipTypewriter();
			return;
		}

		if (_isHidden)
		{
			FinishStandaloneLine();
			return;
		}

		StartFadeOut(view => view.FinishStandaloneLine());
	}

	private void HandleTextShowed()
	{
		if (_isStandaloneLine)
		{
			if (_textCanvasGroup)
			{
				_textCanvasGroup.blocksRaycasts = false;
			}

			StartFadeOut(view => view.FinishStandaloneLine(), _standaloneDismissDelay);
			_dismissSequence.isPaused = !_state.CanAdvanceStory;
			StandaloneTextShown?.Invoke();
		}
		else if (_isHidden)
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
		ClearLine();
		StandaloneLineFinished?.Invoke();
	}

	private void HandleEndStory()
	{
		if (_isHidden)
		{
			ClearLine();
			return;
		}

		StartFadeOut(view => view.ClearLine());
	}

	private void StartStandaloneShow(string line)
	{
		SetTextCanvasGroup(0f, true);

		_showSequence = Sequence.Create();
		ChainFade(_showSequence, _fadeCanvasGroup, 1f, _standaloneBgFadeDuration);
		_showSequence.ChainCallback(() => _typewriter.ShowText(line));
		ChainFade(_showSequence, _textCanvasGroup, 1f, _standaloneTextFadeDuration);
	}

	private void StartFadeOut(Action<TextShowUI> onComplete, float delay = 0f)
	{
		StopActiveSequences();

		_dismissSequence = Sequence.Create();
		if (delay > 0f)
		{
			_dismissSequence.ChainDelay(delay);
		}

		if (!_isHidden)
		{
			_dismissSequence.ChainCallback(this, view => view.BeginFadeOut());
			ChainFade(_dismissSequence, _textCanvasGroup, 0f, _standaloneTextFadeDuration);
			ChainFade(_dismissSequence, _fadeCanvasGroup, 0f, _standaloneBgFadeDuration);
		}

		_dismissSequence.OnComplete(this, onComplete);
	}

	private void BeginFadeOut()
	{
		_isFadingOut = true;
		if (_textCanvasGroup)
		{
			_textCanvasGroup.blocksRaycasts = false;
		}
	}

	private static void ChainFade(Sequence sequence, CanvasGroup group, float targetAlpha, float duration)
	{
		if (group)
		{
			sequence.Chain(Tween.Alpha(group, targetAlpha, duration, Ease.Linear));
		}
	}

	private void StopActiveSequences()
	{
		_showSequence.Stop();
		_dismissSequence.Stop();
		_isFadingOut = false;
	}

	private void ClearLine()
	{
		StopActiveSequences();
		_typewriter.StopShowingText();
		_typewriter.TextAnimator.SetText(string.Empty);
		SetCanvasGroups(0f, false);
	}

	private void SetCanvasGroups(float alpha, bool blocksRaycasts)
	{
		SetTextCanvasGroup(alpha, blocksRaycasts);
		if (_fadeCanvasGroup)
		{
			_fadeCanvasGroup.alpha = alpha;
		}
	}

	private void SetTextCanvasGroup(float alpha, bool blocksRaycasts)
	{
		if (_textCanvasGroup)
		{
			_textCanvasGroup.alpha = alpha;
			_textCanvasGroup.blocksRaycasts = blocksRaycasts;
		}
	}
}
