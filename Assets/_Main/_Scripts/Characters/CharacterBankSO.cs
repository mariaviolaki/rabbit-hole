using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
	[CreateAssetMenu(fileName = "Character Bank", menuName = "Scriptable Objects/Character Bank")]
	public class CharacterBankSO : ScriptableObject
	{
		[SerializeField] CharacterData[] gameCharacters;
		readonly Dictionary<string, CharacterData> characters = new Dictionary<string, CharacterData>();

		public Dictionary<string, CharacterData> Characters { get { return characters; } }

		public bool HasCharacterData(string shortName)
		{
			return characters.ContainsKey(shortName);
		}

		public CharacterData GetCharacterData(string shortName, string castShortName, GameOptionsSO gameOptions)
		{
			if (characters.ContainsKey(castShortName))
				return CharacterData.Get(shortName, characters[castShortName], shortName == castShortName);

			return GetDefaultData(shortName, gameOptions);
		}

		public CharacterData GetDefaultData(string shortName, GameOptionsSO gameOptions)
		{
			return CharacterData.GetDefault(shortName, gameOptions);
		}

		void OnEnable()
		{
			// Convert the array to a dictionary for fast lookups
			foreach (CharacterData character in gameCharacters)
			{
				characters.Add(character.ShortName, character);
			}
		}
	}
}