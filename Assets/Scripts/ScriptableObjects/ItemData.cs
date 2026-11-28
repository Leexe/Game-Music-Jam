using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
	public string ItemName;

	[TextArea]
	public string ClickDescription;

	[PreviewField]
	public Sprite Icon;

	[PreviewField]
	public Sprite PolaroidImage;
}
