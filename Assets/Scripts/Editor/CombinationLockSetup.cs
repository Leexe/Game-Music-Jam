#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

// Select a door (or nothing) and use GameObject > UI > Combination Lock.
// Builds a keypad panel on the root Canvas, wires its buttons, and links it to the selected Door.
public static class CombinationLockSetup
{
	[MenuItem("Tools/Create Combination Lock")]
	private static void Create()
	{
		Door door = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Door>() : null;

		Canvas canvas = door != null ? door.GetComponentInParent<Canvas>() : Object.FindFirstObjectByType<Canvas>();
		if (canvas == null)
		{
			EditorUtility.DisplayDialog("Combination Lock", "No Canvas found in the scene.", "OK");
			return;
		}
		canvas = canvas.rootCanvas; // keep the panel outside the rooms, so hiding a room doesn't hide it

		var res = new TMP_DefaultControls.Resources();

		// Full-screen dim panel that also blocks clicks on the room behind it
		var panel = new GameObject("CombinationLockPanel", typeof(RectTransform), typeof(Image));
		Undo.RegisterCreatedObjectUndo(panel, "Create Combination Lock");
		panel.transform.SetParent(canvas.transform, false);
		panel.transform.SetAsLastSibling();
		var pRect = (RectTransform)panel.transform;
		pRect.anchorMin = Vector2.zero;
		pRect.anchorMax = Vector2.one;
		pRect.offsetMin = pRect.offsetMax = Vector2.zero;
		panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

		// Code input
		GameObject inputGO = TMP_DefaultControls.CreateInputField(res);
		inputGO.name = "CodeInput";
		inputGO.transform.SetParent(panel.transform, false);
		Place(inputGO, new Vector2(0, 40), new Vector2(300, 60));
		var input = inputGO.GetComponent<TMP_InputField>();
		input.contentType = TMP_InputField.ContentType.IntegerNumber;
		input.characterLimit = 4;
		input.textComponent.fontSize = 36;
		input.textComponent.alignment = TextAlignmentOptions.Center;
		if (input.placeholder is TMP_Text ph)
		{
			ph.text = "Enter code";
			ph.alignment = TextAlignmentOptions.Center;
		}

		var lockComp = Undo.AddComponent<CombinationLock>(panel);

		Button enter = MakeButton(res, panel.transform, "EnterButton", "Enter", new Vector2(-80, -50));
		Button close = MakeButton(res, panel.transform, "CloseButton", "X", new Vector2(80, -50));
		UnityEventTools.AddPersistentListener(enter.onClick, lockComp.Submit);
		UnityEventTools.AddPersistentListener(close.onClick, lockComp.Close);

		var so = new SerializedObject(lockComp);
		so.FindProperty("_panel").objectReferenceValue = panel;
		so.FindProperty("_input").objectReferenceValue = input;
		so.ApplyModifiedProperties();

		if (door != null)
		{
			var doorSO = new SerializedObject(door);
			doorSO.FindProperty("_lock").objectReferenceValue = lockComp;
			doorSO.ApplyModifiedProperties();
		}

		Selection.activeGameObject = panel;
		Debug.Log(
			door != null
				? $"Combination lock created and linked to door '{door.name}'. Set the code on CombinationLockPanel."
				: "Combination lock created. Drag CombinationLockPanel into a Door's 'Lock' slot and set the code."
		);
	}

	private static Button MakeButton(TMP_DefaultControls.Resources res, Transform parent, string name, string label, Vector2 pos)
	{
		GameObject go = TMP_DefaultControls.CreateButton(res);
		go.name = name;
		go.transform.SetParent(parent, false);
		Place(go, pos, new Vector2(140, 50));
		go.GetComponentInChildren<TMP_Text>().text = label;
		return go.GetComponent<Button>();
	}

	private static void Place(GameObject go, Vector2 pos, Vector2 size)
	{
		var rt = (RectTransform)go.transform;
		rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
		rt.anchoredPosition = pos;
		rt.sizeDelta = size;
	}
}
#endif
