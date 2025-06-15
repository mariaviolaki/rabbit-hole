using Dialogue;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Characters
{
	public class CharacterManager : MonoBehaviour
	{
		const char CharacterCastDelimiter = ':';

		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] FileManagerSO fileManager;
		[SerializeField] CharacterDirectorySO characterDirectory;
		[SerializeField] DialogueSystem dialogueSystem;
		[SerializeField] RectTransform characterContainer;
		[SerializeField] Transform model3DContainer;

		Dictionary<string, Character> characters = new Dictionary<string, Character>();

		public GameOptionsSO GameOptions { get { return gameOptions; } }
		public CharacterDirectorySO Directory { get { return characterDirectory; } }
		public FileManagerSO FileManager { get { return fileManager; } }
		public DialogueSystem Dialogue { get { return dialogueSystem; } }
		public RectTransform Container { get { return characterContainer; } }
		public Transform Model3DContainer { get { return model3DContainer; } }

		void Awake()
		{
			Character.OnCreateCharacter += (Character character) => SaveCharacter(character);
		}

		public bool HasCharacter(string shortName) => characters.ContainsKey(shortName);

		public IEnumerator CreateCharacters(List<string> names)
		{
			int completedCharacterCount = 0;

			IEnumerator MarkCharacterCompletion(string shortName, string castShortName)
			{
				yield return CreateCharacter(shortName, castShortName);
				completedCharacterCount++;
			}

			for (int i = 0; i < names.Count; i++)
			{
				string[] nameParts = names[i].Split(CharacterCastDelimiter);
				string shortName = nameParts[0].Trim();
				string castShortName = nameParts.Length > 1 ? nameParts[1] : shortName;

				StartCoroutine(MarkCharacterCompletion(shortName, castShortName));
			}

			while (completedCharacterCount != names.Count) yield return null;
		}

		public IEnumerator CreateCharacter(string shortName) => CreateCharacter(shortName, shortName);
		public IEnumerator CreateCharacter(string shortName, string castShortName)
		{
			if (characters.ContainsKey(shortName))
			{
				Debug.LogWarning($"Unable to create character '{shortName}' because they already exist.");
				yield break;
			}
			else if (!IsValidShortName(shortName) || !IsValidShortName(castShortName))
			{
				Debug.LogWarning($"Unable to create character '${shortName}' because their short name is invalid.");
				yield break;
			}

			castShortName = string.IsNullOrWhiteSpace(castShortName) ? shortName : castShortName;
			CharacterData data = characterDirectory.GetCharacterData(shortName, castShortName, gameOptions);

			switch (data.Type)
			{
				case CharacterType.Model3D:
					yield return Character.Create<Model3DCharacter>(this, data);
					break;
				case CharacterType.Sprite:
					yield return Character.Create<SpriteCharacter>(this, data);
					break;
				case CharacterType.Text:
				default:
					yield return Character.Create<TextCharacter>(this, data);
					break;
			}
		}

		public Character GetCharacter(string shortName)
		{
			if (string.IsNullOrEmpty(shortName)) return null;

			if (!characters.ContainsKey(shortName))
				CreateDefaultCharacter(shortName);

			return characters[shortName];
		}

		public int GetCharacterCount(CharacterType characterType)
		{
			int count = 0;

			foreach (Character character in characters.Values)
			{
				if (character.Data.Type == characterType)
					count++;
			}

			return count;
		}

		public bool StopProcess(ref Coroutine process)
		{
			if (process == null) return false;

			StopCoroutine(process);
			process = null;

			return true;
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

		public bool SetCharacterShortName(string oldShortName, string newShortName)
		{
			if (characters.ContainsKey(newShortName))
			{
				Debug.LogWarning($"Unable to set character short name '{newShortName}' because it already exists.");
				return false;
			}

			characters[newShortName] = characters[oldShortName];
			characters.Remove(oldShortName);
			return true;
		}

		// Called when a character is initialized asyncronously
		void SaveCharacter(Character character)
		{
			characters[character.Data.ShortName] = character;
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

		void CreateDefaultCharacter(string shortName)
		{
			CharacterData data = characterDirectory.GetDefaultData(shortName, gameOptions);
			characters[shortName] = Character.CreateDefault(this, data);
		}

		bool IsValidShortName(string input)
		{
			return Regex.IsMatch(input, @"^[_a-zA-Z][_a-zA-Z0-9]*$");
		}
	}
}
