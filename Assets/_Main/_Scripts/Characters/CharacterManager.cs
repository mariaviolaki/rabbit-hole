using Dialogue;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
	public class CharacterManager : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] CharacterDirectorySO characterDirectory;
		[SerializeField] DialogueSystem dialogueSystem;
		[SerializeField] RectTransform characterContainer;

		Dictionary<string, Character> characters = new Dictionary<string, Character>();

		public GameOptionsSO GameOptions { get { return gameOptions; } }
		public CharacterDirectorySO Directory { get { return characterDirectory; } }
		public DialogueSystem Dialogue { get { return dialogueSystem; } }

		public Character GetCharacter(string name)
		{
			if (name == null) return null;

			if (!characters.ContainsKey(name))
				CreateCharacter(name);

			return characters[name];
		}

		void CreateCharacter(string name)
		{
			CharacterData data = characterDirectory.GetCharacterData(name, gameOptions);

			switch (data.Type)
			{
				case CharacterType.Sprite:
					characters[name] = new SpriteCharacter(name, this);
					break;
				case CharacterType.Text:
				default:
					characters[name] = new TextCharacter(name, this);
					break;
			}
		}
	}
}
