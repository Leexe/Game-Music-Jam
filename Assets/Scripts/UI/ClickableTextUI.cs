using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickableTextUI : MonoBehaviour
{
	[SerializeField]
	private string _textDisplay = "";

	public void DisplayText()
	{
		DialogueManager.Instance.DisplayLoneLine(_textDisplay);
	}
}
