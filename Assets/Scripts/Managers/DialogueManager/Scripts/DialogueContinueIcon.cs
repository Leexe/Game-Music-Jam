using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Controls the appearance and disappearance of the dialogue continue icon,
/// listening to typewriter and dialogue state events.
/// </summary>
public class DialogueContinueIcon : MonoBehaviour
{
	[FoldoutGroup("References")]
	[SerializeField]
	private DialogueManager _dialogueManager;

	[FoldoutGroup("References")]
	[SerializeField]
	private TextShowUI _textView;

	[FoldoutGroup("References")]
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[FoldoutGroup("Animation Settings")]
	[SerializeField]
	private float _appearDuration = 0.25f;

	[FoldoutGroup("Animation Settings")]
	[SerializeField]
	private float _disappearDuration = 0.15f;

	private Tween _fadeTween;
	private DialogueState _dialogueState;

	private void Awake()
	{
		SetHiddenImmediate();
	}

	private void OnEnable()
	{
		if (_dialogueManager != null)
		{
			_dialogueState = _dialogueManager.DialogueState;
			_dialogueState.OnTypewriterFinish += ShowIcon;
			_textView.StandaloneTextShown += ShowIcon;
			_textView.StandaloneLineFinished += HideIcon;
			_dialogueState.OnStartDialogue += HideIcon;
			_dialogueState.OnDisplayDialogue += HideIcon;
			_dialogueState.OnEndStory += HideIcon;
		}
	}

	private void OnDisable()
	{
		if (_dialogueState != null)
		{
			_dialogueState.OnTypewriterFinish -= ShowIcon;
			_textView.StandaloneTextShown -= ShowIcon;
			_textView.StandaloneLineFinished -= HideIcon;
			_dialogueState.OnStartDialogue -= HideIcon;
			_dialogueState.OnDisplayDialogue -= HideIcon;
			_dialogueState.OnEndStory -= HideIcon;
		}
		StopAllTweens();
	}

	private void HideIcon(string speakerName, string text) => HideIcon();

	[Button]
	public void ShowIcon()
	{
		StopAllTweens();
		if (_canvasGroup != null)
		{
			_fadeTween = Tween.Alpha(_canvasGroup, 1f, _appearDuration, Ease.Linear);
		}
	}

	[Button]
	public void HideIcon()
	{
		StopAllTweens();
		if (_canvasGroup != null)
		{
			_fadeTween = Tween.Alpha(_canvasGroup, 0f, _disappearDuration, Ease.Linear);
		}
	}

	private void SetHiddenImmediate()
	{
		StopAllTweens();
		if (_canvasGroup != null)
		{
			_canvasGroup.alpha = 0f;
		}
	}

	private void StopAllTweens()
	{
		_fadeTween.Stop();
	}
}
