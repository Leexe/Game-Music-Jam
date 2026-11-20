using UnityEngine;
using UnityEngine.UI;



//Put this on a door Button. Clicking it smoothly enters the target room.


[RequireComponent(typeof(Button))]
public class Door : MonoBehaviour
{
	[Tooltip("The room this door leads to.")]
	[SerializeField]
	private GameObject _targetRoom;

	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(Enter);
	}

	private void Enter()
	{
		RoomManager.Instance.EnterRoom(_targetRoom, (RectTransform)transform);
	}
}
