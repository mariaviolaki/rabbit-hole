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

		// Give a character a specific priority, ignoring invisible characters
		// Characters with small indices will be rendered on top
		public void SetPriority(string name, int index)
		{
			if (name == null || !characters.ContainsKey(name) || characters[name].Root == null) return;

			// Position any invisible characters to the very back
			int invisibleCount = SortInvisibleCharacters();

			// Reverse the index and render this character on top of any invisibles
			int lastIndex = characterContainer.childCount - 1;
			int clampedIndex = Mathf.Clamp(index, 0, lastIndex);
			int siblingIndex = lastIndex - clampedIndex;
			int clampedSiblingIndex = Mathf.Clamp(siblingIndex, invisibleCount, lastIndex);

			// Automatically set the index of any other visible characters
			characters[name].Root.SetSiblingIndex(clampedSiblingIndex);
		}

		// These characters will be placed on top of all the others (in the given order)
		public void SetPriority(string[] characterNames)
		{
			// Position any invisible characters to the very back
			SortInvisibleCharacters();

			for (int i = characterNames.Length - 1; i >= 0; i--)
			{
				string name = characterNames[i];
				if (name == null || !characters.ContainsKey(name) || characters[name].Root == null) continue;

				// Position all the given characters on top in the order they were given
				characters[name].Root.SetSiblingIndex(characterContainer.childCount);
			}
		}

		int SortInvisibleCharacters()
		{
			int characterCount = 0;

			foreach (Character character in characters.Values)
			{
				// This character doesn't have a transform in the hierarchy
				if (!character.Root) continue;

				// Position all invisible characters at the end (order doesn't matter)
				bool isInvisible = !character.Root.gameObject.activeInHierarchy || !character.IsVisible;
				if (isInvisible)
				{
					characterCount++;
					character.Root.SetSiblingIndex(0);
				}
			}

			return characterCount;
		}

		void CreateDefaultCharacter(string name)
		{
			CharacterData data = characterDirectory.GetDefaultData(name, gameOptions);
			characters[name] = Character.CreateDefault(this, data);
		}
	}
}
