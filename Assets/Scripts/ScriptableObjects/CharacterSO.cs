using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Dialogue/CharacterSO", order = 0)]
public class CharacterSO : ScriptableObject
{
	public string Name;
	public string Id;
	public Color ColorTheme = Color.black;
	public Sprite Icon;

	public static Dictionary<string, CharacterSO> BuildLookup(IEnumerable<CharacterSO> characters)
	{
		var lookup = new Dictionary<string, CharacterSO>(StringComparer.OrdinalIgnoreCase);
		foreach (CharacterSO character in characters)
		{
			lookup[character.Id] = character;
			lookup[character.Name] = character;
		}
		return lookup;
	}

	public static CharacterSO ResolveFromLine(
		string line,
		IReadOnlyDictionary<string, CharacterSO> lookup,
		CharacterSO fallback = null
	)
	{
		int colonIndex = line.IndexOf(':');
		if (colonIndex > 0)
		{
			string speakerId = line.Substring(0, colonIndex).Trim();
			if (lookup.TryGetValue(speakerId, out CharacterSO character))
			{
				return character;
			}
		}

		return fallback;
	}
}
