using FMODUnity;
using TMPro;
using UnityEngine;

public class DialogueChoiceBox : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _choiceBoxText;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[Tooltip("Optional click sound effect. Leave empty for no sound.")]
	[SerializeField]
	private EventReference _clickSfx;

	private DialogueManager _dialogueManager;
	private int _choiceIndex;

	public CanvasGroup CanvasGroup => _canvasGroup;

	private void Reset()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
	}

	public void SetText(string text)
	{
		_choiceBoxText.text = text;
	}

	public void SetChoiceIndex(int index)
	{
		_choiceIndex = index;
	}

	public void OnChoicePressed()
	{
		if (!_clickSfx.IsNull)
		{
			AudioManager.Instance.PlayOneShot(_clickSfx);
		}

		DialogueManager manager = _dialogueManager ? _dialogueManager : DialogueManager.Instance;
		manager.DialogueState.OnChoiceSelect?.Invoke(_choiceIndex);
	}
}
