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

	public void DisplayText()
	{
		DialogueManager.Instance.DisplayLoneLine(_textDisplay);
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
