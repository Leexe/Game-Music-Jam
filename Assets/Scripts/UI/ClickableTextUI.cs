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
	[SerializeField]
	private string _textDisplay = "";

	[SerializeField]
	private string _interactionId = "";

	public void DisplayText()
	{
		if (CameraItem.ShouldIgnoreClick)
		{
			return;
		}

		string interactionId = _interactionId.Trim();
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
