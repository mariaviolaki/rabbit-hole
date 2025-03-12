using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
	public class CharacterManager : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] CharacterDirectorySO characterDirectory;
		[SerializeField] RectTransform characterContainer;

		Dictionary<string, Character> characters = new Dictionary<string, Character>();

		public Character GetCharacter(string name)
		{
			if (!characters.ContainsKey(name))
				CreateCharacter(name);

			return characters[name];
		}

		void CreateCharacter(string name)
		{
			CharacterData data = characterDirectory.GetCharacterData(name, gameOptions);

			switch (data.type)
			{
				case CharacterType.Sprite:
					characters[name] = new SpriteCharacter(name, characterDirectory, gameOptions);
					break;
				case CharacterType.Text:
				default:
					characters[name] = new TextCharacter(name, characterDirectory, gameOptions);
					break;
			}
		}
	}
}
