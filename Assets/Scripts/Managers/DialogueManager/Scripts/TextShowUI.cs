using Febucci.TextAnimatorForUnity;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TypewriterComponent))]
public class TextShowUI : MonoBehaviour
{
	[Header("Behaviour")]
	[SerializeField]
	private bool _isPrimary;

	[SerializeField]
	private bool _clearStandaloneLineOnEnd = true;

	private TypewriterComponent _typewriter;
	private DialogueState _state;
	private bool _isStandaloneLine;

	private void Reset()
	{
		_typewriter = GetComponent<TypewriterComponent>();
	}

	private void OnEnable()
	{
		_state = DialogueManager.Instance.DialogueState;

		_state.OnDisplayDialogue += ShowLine;
		_state.OnTypewriterSkip += _typewriter.SkipTypewriter;
		_state.OnEndStory += HandleDialogueEnded;
		DialogueManager.Instance.OnDialogueStarted += HandleDialogueStarted;

		if (_isPrimary)
		{
			_typewriter.onTextShowed.AddListener(_state.TypewriterFinished);
		}
	}

	private void OnDisable()
	{
		_state.OnDisplayDialogue -= ShowLine;
		_state.OnTypewriterSkip -= _typewriter.SkipTypewriter;
		_state.OnEndStory -= HandleDialogueEnded;
		DialogueManager.Instance.OnDialogueStarted -= HandleDialogueStarted;

		_typewriter.onTextShowed.RemoveListener(_state.TypewriterFinished);
	}

	private void ShowLine(string speaker, string line)
	{
		_typewriter.ShowText(line);
	}

	private void HandleDialogueStarted(string knotName)
	{
		// TryShowLine passes null. Ink stories pass their knot name.
		_isStandaloneLine = knotName == null;
	}

	private void HandleDialogueEnded()
	{
		if (_isStandaloneLine && _clearStandaloneLineOnEnd)
		{
			_typewriter.TextAnimator.SetText(string.Empty);
		}
	}
}
