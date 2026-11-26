using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Dialogue/CharacterSO", order = 0)]
public class CharacterSO : ScriptableObject
{
	public string Name;
	public string Id;
	public Color ColorTheme = Color.black;
	public Sprite Icon;
}
