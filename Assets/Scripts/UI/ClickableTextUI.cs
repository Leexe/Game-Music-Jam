using System;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

[RequireComponent(typeof(Button))]
public class ClickableTextUI : MonoBehaviour
{
	public enum ClickSoundType
	{
		None,
		PaperRustling,
		Keys,
	}

	[SerializeField]
	private string _textDisplay = "";

	[SerializeField]
	private string _interactionId = "";

	[SerializeField]
	private ClickSoundType _clickSound = ClickSoundType.None;

	public void DisplayText()
	{
		if (CameraItem.ShouldIgnoreClick)
		{
			return;
		}

		string interactionId = _interactionId.Trim();
		PlayClickSound(interactionId);
		if (!string.IsNullOrEmpty(interactionId))
		{
			DialogueManager.Instance.RecordInteraction(interactionId);
		}

		if (!string.IsNullOrWhiteSpace(_textDisplay))
		{
			DialogueManager.Instance.DisplayLoneLine(_textDisplay);
		}
		else if (!DialogueManager.Instance.IsStoryPlaying && DialogueManager.Instance.HasKnot(interactionId))
		{
			DialogueManager.Instance.DialogueState.StartStory(interactionId);
		}
	}

	private void PlayClickSound(string interactionId)
	{
		EventReference sound = _clickSound switch
		{
			ClickSoundType.PaperRustling => FMODEvents.Instance.PaperRustling_Sfx,
			ClickSoundType.Keys => FMODEvents.Instance.Keys_Sfx,
			_ => ResolveSoundByInteractionId(interactionId),
		};

		if (!sound.IsNull)
		{
			AudioManager.Instance.PlayOneShot(sound);
		}
	}

	private static EventReference ResolveSoundByInteractionId(string interactionId)
	{
		if (
			interactionId.Equals("love_letter", StringComparison.OrdinalIgnoreCase)
			|| interactionId.Equals("ned_card", StringComparison.OrdinalIgnoreCase)
			|| interactionId.Equals("ast_book", StringComparison.OrdinalIgnoreCase)
		)
		{
			return FMODEvents.Instance.PaperRustling_Sfx;
		}

		if (
			interactionId.Equals("key", StringComparison.OrdinalIgnoreCase)
			|| interactionId.Equals("keys", StringComparison.OrdinalIgnoreCase)
		)
		{
			return FMODEvents.Instance.Keys_Sfx;
		}

		return default;
	}

#if UNITY_EDITOR
	private void Reset()
	{
		ConnectButtonToDisplayText();
	}

	private bool HasDisplayTextListener(Button button)
	{
		for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
		{
			if (
				button.onClick.GetPersistentTarget(i) == this
				&& button.onClick.GetPersistentMethodName(i) == nameof(DisplayText)
			)
			{
				return true;
			}
		}

		return false;
	}

	[HideInPlayMode]
	[Button("Add Function To Button")]
	private void ConnectButtonToDisplayText()
	{
		Button button = GetComponent<Button>();
		if (HasDisplayTextListener(button))
		{
			return;
		}

		Undo.RecordObject(button, "Connect Clickable Text Button");
		UnityEventTools.AddPersistentListener(button.onClick, DisplayText);
		PrefabUtility.RecordPrefabInstancePropertyModifications(button);
	}
#endif
}
