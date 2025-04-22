using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
	[CreateAssetMenu(fileName = "Character Directory", menuName = "Scriptable Objects/Character Directory")]
	public class CharacterDirectorySO : ScriptableObject
	{
		[SerializeField] CharacterData[] gameCharacters;
		Dictionary<string, CharacterData> characters = new Dictionary<string, CharacterData>();

		public Dictionary<string, CharacterData> Characters { get { return characters; } }

		public bool HasCharacterData(string name)
		{
			return characters.ContainsKey(name);
		}

		public CharacterData GetCharacterData(string name, string castName, GameOptionsSO gameOptions)
		{
			if (characters.ContainsKey(castName))
				return CharacterData.Get(name, characters[castName], name == castName);

			return GetDefaultData(name, gameOptions);
		}

		public CharacterData GetDefaultData(string name, GameOptionsSO gameOptions)
		{
			return CharacterData.GetDefault(name, gameOptions);
		}

		void OnEnable()
		{
			// Convert the array to a dictionary for fast lookups
			foreach (CharacterData character in gameCharacters)
			{
				characters.Add(character.Name, character);
			}
		}
	}
}