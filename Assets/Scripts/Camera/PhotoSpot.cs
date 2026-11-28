using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A clue in the room. Click it while holding the camera to see a photo of the past.
/// After the photo, it disappears and goes into the inventory.
/// </summary>
public class PhotoSpot : MonoBehaviour
{
	[Tooltip("Optional ItemData asset for this clue (provides Inventory Icon and Polaroid Image).")]
	[SerializeField]
	private ItemData _itemData;

	[Tooltip("The PNG shown in the polaroid (the past).")]
	[SerializeField]
	private Sprite _pastImage;

	[Tooltip("Optional text written under the polaroid.")]
	[SerializeField]
	[TextArea]
	private string _caption;

	[Tooltip("Icon shown in the inventory. Leave empty to use this object's own Image.")]
	[SerializeField]
	private Sprite _inventoryIcon;

	[Tooltip("Hide this item from the room after it's photographed.")]
	[SerializeField]
	private bool _hideAfterPhoto = false;

	public ItemData ItemData => _itemData;

	public Sprite PastImage => _itemData != null && _itemData.PolaroidImage != null ? _itemData.PolaroidImage : _pastImage;

	public string Caption => _caption;

	public Sprite InventoryIcon => _itemData != null && _itemData.Icon != null
		? _itemData.Icon
		: (_inventoryIcon != null ? _inventoryIcon : GetComponent<Image>().sprite);

	public bool HasBeenPhotographed { get; private set; }

	public void MarkPhotographed()
	{
		HasBeenPhotographed = true;

		if (_hideAfterPhoto)
		{
			gameObject.SetActive(false);
		}
	}
}
