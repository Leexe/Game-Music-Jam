using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A photographed item in the inventory. Click it to see its polaroid again.
/// </summary>
[RequireComponent(typeof(DraggableUI))]
public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
	[SerializeField]
	private Image _icon;

	public Sprite PastImage { get; private set; }

	public string Caption { get; private set; }

	private void Reset()
	{
		_icon = GetComponent<Image>();
	}

	public void Setup(PhotoSpot spot)
	{
		if (_icon == null)
		{
			_icon = GetComponent<Image>();
		}

		_icon.sprite = spot.InventoryIcon;
		PastImage = spot.PastImage;
		Caption = spot.Caption;
		name = spot.name;

		if (spot.ItemData != null)
		{
			GetComponent<DraggableUI>().SetItemData(spot.ItemData);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		// Ignore the click at the end of a drag
		if (eventData.dragging)
		{
			return;
		}

		CameraManager.Instance.ShowPolaroid(PastImage, Caption);
	}
}
