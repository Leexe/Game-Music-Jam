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
		InventorySlotUI freeSlot = FindEmptySlot();
		if (freeSlot == null)
		{
			return false;
		}

		return freeSlot.AddItem(item);
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
