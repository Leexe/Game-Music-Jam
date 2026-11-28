using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

//Put this on a door Button. Clicking it smoothly enters the target room.

[RequireComponent(typeof(Button))]
public class Door : MonoBehaviour
{
	[Tooltip("The room this door leads to.")]
	[SerializeField]
	private GameObject _targetRoom;

	[SerializeField]
	private CombinationLock _lock;

	private Button _button;

	private void OnEnable()
	{
		_button = GetComponent<Button>();
		if (!HasEnterListener(_button))
		{
			_button.onClick.AddListener(Enter);
		}
	}

	private void OnDisable()
	{
		if (_button != null)
		{
			_button.onClick.RemoveListener(Enter);
		}
	}

	private bool HasEnterListener(Button button)
	{
		for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
		{
			if (
				button.onClick.GetPersistentTarget(i) == this
				&& button.onClick.GetPersistentMethodName(i) == nameof(Enter)
			)
			{
				return true;
			}
		}

		return false;
	}

#if UNITY_EDITOR
	[HideInPlayMode]
	[Button("Add Function To Button")]
	private void ConnectButtonToEnter()
	{
		Button button = GetComponent<Button>();
		if (HasEnterListener(button))
		{
			return;
		}

		Undo.RecordObject(button, "Connect Door Button");
		UnityEventTools.AddPersistentListener(button.onClick, Enter);
		PrefabUtility.RecordPrefabInstancePropertyModifications(button);
	}
#endif

	public void Enter()
	{
		if (_lock != null && _lock.IsLocked)
		{
			_lock.Open(this);
			return;
		}
		RoomManager.Instance.EnterRoom(_targetRoom, (RectTransform)transform);
	}
}
