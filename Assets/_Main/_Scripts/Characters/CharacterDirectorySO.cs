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

		public CharacterData GetCharacterData(string name, GameOptionsSO gameOptions)
		{
			if (characters.ContainsKey(name))
				return characters[name];

			CharacterData defaultData = CharacterData.GetDefault(gameOptions);
			defaultData.name = name;

			return defaultData;
		}

		void OnEnable()
		{
			// Convert the array to a dictionary for fast lookups
			foreach (CharacterData character in gameCharacters)
			{
				characters.Add(character.name, character);
			}
		}
	}
}