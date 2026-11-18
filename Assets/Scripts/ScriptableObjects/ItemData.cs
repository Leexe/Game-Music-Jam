using UnityEngine;

namespace ScriptableObjects
{
	[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/ItemData", order = 0)]
	public class ItemData : ScriptableObject
	{
		public string ItemName;
		public string ItemDescription;
		public Sprite Icon;
	}
}
