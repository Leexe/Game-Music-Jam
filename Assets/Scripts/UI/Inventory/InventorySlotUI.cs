using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public GameObject StoredItem { get; private set; }
	public bool IsUsed { get; private set; }

	public bool AddItem(GameObject item)
	{
		if (IsUsed)
		{
			return false;
		}

		IsUsed = true;
		StoredItem = item;
		item.transform.SetParent(transform);

		return true;
	}

	public void RemoveItem()
	{
		if (!IsUsed)
		{
			return;
		}

		IsUsed = false;
		StoredItem = null;
	}

	public void OnBeginDrag(PointerEventData eventData) { }

	public void OnDrag(PointerEventData eventData)
	{
		throw new System.NotImplementedException();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		throw new System.NotImplementedException();
	}
}
