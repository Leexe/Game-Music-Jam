using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
	[SerializeField]
	private List<InventorySlotUI> _invSlots;

	public void AddItemToInventory(GameObject item)
	{
		InventorySlotUI slot = FindEmptySlot();
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
}
