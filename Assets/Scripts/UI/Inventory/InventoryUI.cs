using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
	[SerializeField]
	[ReadOnly]
	private List<InventorySlotUI> _invSlots;

	public bool AddItemToInventory(DraggableUI item)
	{
		foreach (InventorySlotUI _ in _invSlots)
		{
			InventorySlotUI freeSlot = FindEmptySlot();
			freeSlot.AddItem(item);
		}

		return false;
	}

	private InventorySlotUI FindEmptySlot()
	{
		foreach (InventorySlotUI slot in _invSlots)
		{
			if (!slot.IsUsed)
			{
				return slot;
			}
		}

		return null;
	}

	[Button("Find Inventory Slots")]
	private void FindInventorySlots()
	{
		_invSlots = new List<InventorySlotUI>(GetComponentsInChildren<InventorySlotUI>());
	}
}
