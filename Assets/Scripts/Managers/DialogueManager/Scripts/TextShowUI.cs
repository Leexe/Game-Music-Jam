using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorForUnity;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(TypewriterComponent))]
[RequireComponent(typeof(TextAnimator))]
public class TextShowUI : MonoBehaviour
{
	[Header("Behaviour")]
	[InfoBox("Only the hidden typewriter reports line completion to the dialogue manager.")]
	[SerializeField]
	private bool _isPrimary;

	[SerializeField]
	private bool _clearStandaloneLineOnEnd = true;

	[SerializeField]
	private TypewriterComponent _typewriter;

	private DialogueManager _dialogueManager;
	private DialogueState _state;
	private bool _isStandaloneLine;

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
			_state.OnEndStory += HandleDialogueEnded;
			_dialogueManager.OnDialogueStarted += HandleDialogueStarted;

			if (_isPrimary)
			{
				_typewriter.onTextShowed.AddListener(_state.TypewriterFinished);
			}
		}
	}

	private void OnDisable()
	{
		if (_dialogueManager)
		{
			_state.OnDisplayDialogue -= ShowLine;
			_state.OnTypewriterSkip -= _typewriter.SkipTypewriter;
			_state.OnEndStory -= HandleDialogueEnded;
			_dialogueManager.OnDialogueStarted -= HandleDialogueStarted;
			_typewriter.onTextShowed.RemoveListener(_state.TypewriterFinished);
		}
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
