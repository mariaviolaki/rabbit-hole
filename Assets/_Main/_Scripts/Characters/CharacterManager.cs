using Dialogue;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Characters
{
	public class CharacterManager : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] FileManagerSO fileManager;
		[SerializeField] CharacterDirectorySO characterDirectory;
		[SerializeField] DialogueSystem dialogueSystem;
		[SerializeField] RectTransform characterContainer;

		Dictionary<string, Character> characters = new Dictionary<string, Character>();

		public GameOptionsSO GameOptions { get { return gameOptions; } }
		public CharacterDirectorySO Directory { get { return characterDirectory; } }
		public FileManagerSO FileManager { get { return fileManager; } }
		public DialogueSystem Dialogue { get { return dialogueSystem; } }
		public RectTransform Container { get { return characterContainer; } }

		public Task CreateCharacter(string name) => CreateCharacter(name, name);
		public async Task CreateCharacter(string name, string castName)
		{
			CharacterData data = characterDirectory.GetCharacterData(name, castName, gameOptions);

			switch (data.Type)
			{
				case CharacterType.Sprite:
					characters[name] = await Character.Create<SpriteCharacter>(this, data);
					break;
				case CharacterType.Text:
				default:
					characters[name] = await Character.Create<TextCharacter>(this, data);
					break;
			}
		}

		public Character GetCharacter(string name)
		{
			if (name == null) return null;

			if (!characters.ContainsKey(name))
				CreateDefaultCharacter(name);

			return characters[name];
		}

		public void StopProcess(ref Coroutine process)
		{
			if (process == null) return;

			StopCoroutine(process);
			process = null;
		}

		void CreateDefaultCharacter(string name)
		{
			CharacterData data = characterDirectory.GetDefaultData(name, gameOptions);
			characters[name] = Character.CreateDefault(this, data);
		}
	}
}
