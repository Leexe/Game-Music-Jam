using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

public class DialogueChoicesController : MonoBehaviour
{
	[Title("References")]
	[SerializeField]
	private GameObject _dialogueBoxPrefab;

	[Title("Fade")]
	[SerializeField, Min(0f)]
	private float _fadeDuration = 0.2f;

	[SerializeField, Min(0f)]
	private float _staggerDelay = 0.08f;

	[SerializeField]
	private Ease _fadeEase = Ease.OutQuad;

	[SerializeField]
	private bool _useUnscaledTime = true;

	private DialogueManager _dialogueManager;
	private Sequence _fadeSequence;
	private readonly List<DialogueChoiceBox> _choiceBoxes = new();

	private void OnEnable()
	{
		_dialogueManager = DialogueManager.Instance;
		_dialogueManager.DialogueState.OnDisplayChoices += DisplayChoices;
		_dialogueManager.DialogueState.OnChoiceSelect += HideChoices;
		_dialogueManager.DialogueState.OnStartStory += HideChoices;
	}

	private void OnDisable()
	{
		_fadeSequence.Stop();
		if (_dialogueManager)
		{
			_dialogueManager.DialogueState.OnDisplayChoices -= DisplayChoices;
			_dialogueManager.DialogueState.OnChoiceSelect -= HideChoices;
			_dialogueManager.DialogueState.OnStartStory -= HideChoices;
		}
	}

	private void DisplayChoices(List<string> choices)
	{
		_fadeSequence.Stop();

		// Instantiate new choice boxes when needed
		int boxesNeeded = Mathf.Max(choices.Count - _choiceBoxes.Count, 0);
		for (int i = 0; i < boxesNeeded; i++)
		{
			_choiceBoxes.Add(Instantiate(_dialogueBoxPrefab, transform).GetComponent<DialogueChoiceBox>());
		}

		_fadeSequence = Sequence.Create(useUnscaledTime: _useUnscaledTime);

		for (int i = 0; i < _choiceBoxes.Count; i++)
		{
			DialogueChoiceBox box = _choiceBoxes[i];
			if (i < choices.Count)
			{
				box.gameObject.SetActive(true);
				box.SetText(choices[i]);
				box.SetChoiceIndex(i);

				CanvasGroup group = box.CanvasGroup;
				group.alpha = 0f;
				group.interactable = false;
				group.blocksRaycasts = false;

				_fadeSequence.Insert(
					i * _staggerDelay,
					Tween
						.Alpha(group, 1f, _fadeDuration, _fadeEase, useUnscaledTime: _useUnscaledTime)
						.OnComplete(
							group,
							static g =>
							{
								g.interactable = true;
								g.blocksRaycasts = true;
							}
						)
				);
			}
			else
			{
				box.gameObject.SetActive(false);
			}
		}
	}

	private void HideChoices(string knotName)
	{
		HideChoices();
	}

	private void HideChoices(int choiceIndex = 0)
	{
		_fadeSequence.Stop();
		foreach (DialogueChoiceBox choiceBox in _choiceBoxes)
		{
			choiceBox.gameObject.SetActive(false);
		}
	}
}
