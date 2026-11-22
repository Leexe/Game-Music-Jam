using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IDropHandler
{
	[SerializeField]
	private RectTransform _rectTransform;

	public DraggableUI StoredItem { get; private set; }
	public bool IsUsed { get; private set; }

	private void Reset()
	{
		_rectTransform = GetComponent<RectTransform>();
	}

	public bool AddItem(DraggableUI item)
	{
		if (!item)
		{
			return false;
		}

		// Dropped back into its own slot
		if (StoredItem == item)
		{
			RealignItem();
			return false;
		}

		// You can't swap an item if the source inventory slot is empty
		if (IsUsed && item.InventorySlot == null)
		{
			return false;
		}

		if (item.InventorySlot)
		{
			SwapItem(item);
		}

		IsUsed = true;
		StoredItem = item;
		item.SetInventorySlot(this);
		RealignItem();
		return true;
	}

	public void SwapItem(DraggableUI item)
	{
		InventorySlotUI sourceSlot = item.InventorySlot;

		sourceSlot.RemoveItem();
		sourceSlot.AddItem(StoredItem);
		RemoveItem();
	}

	public void RemoveItem()
	{
		if (!IsUsed)
		{
			return;
		}

		IsUsed = false;
		StoredItem.RemoveFromInventorySlot();
		StoredItem = null;
	}

	public void RealignItem()
	{
		if (!StoredItem)
		{
			return;
		}

		RectTransform itemRect = StoredItem.GetComponent<RectTransform>();
		itemRect.SetParent(_rectTransform, false);
		itemRect.anchoredPosition = Vector2.zero;
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.pointerDrag != null)
		{
			AddItem(eventData.pointerDrag.GetComponent<DraggableUI>());
		}
	}
}
