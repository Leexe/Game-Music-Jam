using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When a photo is taken, puts that item into the inventory (once per item).
/// </summary>
public class PhotoInventoryCollector : MonoBehaviour
{
	[Tooltip("Leave empty to find the inventory automatically.")]
	[SerializeField]
	private InventoryUI _inventory;

	[Tooltip("Prefab with Image + DraggableUI + InventoryItem.")]
	[SerializeField]
	private InventoryItem _itemPrefab;

	private readonly HashSet<PhotoSpot> _collected = new HashSet<PhotoSpot>();

	private void Start()
	{
		if (_inventory == null)
		{
			_inventory = FindFirstObjectByType<InventoryUI>();
		}

		CameraManager.Instance.OnPhotoTaken += HandlePhotoTaken;
	}

	private void OnDestroy()
	{
		if (CameraManager.Instance != null)
		{
			CameraManager.Instance.OnPhotoTaken -= HandlePhotoTaken;
		}
	}

	private void HandlePhotoTaken(PhotoSpot spot)
	{
		if (_inventory == null || _collected.Contains(spot))
		{
			return;
		}

		InventoryItem item = Instantiate(_itemPrefab, _inventory.transform);
		item.Setup(spot);

		if (_inventory.AddItemToInventory(item.GetComponent<DraggableUI>()))
		{
			_collected.Add(spot);
		}
		else
		{
			Destroy(item.gameObject);
			Debug.LogWarning("Inventory is full!");
		}
	}
}
