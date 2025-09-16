using Dialogue;
using IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Characters
{
	public class CharacterManager : MonoBehaviour
	{
		const char CharacterCastDelimiter = ':';

		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] CharacterBankSO characterBank;
		[SerializeField] GameObject textCharacterPrefab;
		[SerializeField] GameObject spriteCharacterPrefab;
		[SerializeField] GameObject model3DCharacterPrefab;
		[SerializeField] DialogueManager dialogueManager;
		[SerializeField] RectTransform characterContainer;
		[SerializeField] Transform model3DContainer;

		readonly Dictionary<string, Character> characters = new();

		public VNOptionsSO Options => vnOptions;
		public CharacterBankSO Bank => characterBank;
		public AssetManagerSO Assets => assetManager;
		public DialogueManager Dialogue => dialogueManager;
		public RectTransform Container => characterContainer;
		public Transform Model3DContainer => model3DContainer;

		public Dictionary<string, Character> GetCharacters() => characters;
		public bool HasCharacter(string shortName) => characters.ContainsKey(shortName);

		public Character GetCharacter(string shortName)
		{
			if (string.IsNullOrWhiteSpace(shortName)) return null;

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

			castShortName = string.IsNullOrWhiteSpace(castShortName) ? shortName : castShortName;
			if (!IsValidShortName(shortName) || !IsValidShortName(castShortName))
			{
				Debug.LogWarning($"Unable to create character '{shortName}' because their short name is invalid.");
				yield break;
			}

			CharacterData data = characterBank.GetCharacterData(shortName, castShortName, vnOptions);

			switch (data.Type)
			{
				case CharacterType.Model3D:
					GameObject model3DCharacterGameObject = Instantiate(model3DCharacterPrefab, characterContainer, false);
					Model3DCharacter model3DCharacter = model3DCharacterGameObject.GetComponent<Model3DCharacter>();
					yield return model3DCharacter.Initialize(this, data);
					characters[data.ShortName] = model3DCharacter;
					break;
				case CharacterType.Sprite:
					GameObject spriteCharacterGameObject = Instantiate(spriteCharacterPrefab, characterContainer, false);
					SpriteCharacter spriteCharacter = spriteCharacterGameObject.GetComponent<SpriteCharacter>();
					yield return spriteCharacter.Initialize(this, data);
					characters[data.ShortName] = spriteCharacter;
					break;
				case CharacterType.Text:
				default:
					GameObject textCharacterGameObject = Instantiate(textCharacterPrefab, characterContainer, false);
					TextCharacter textCharacter = textCharacterGameObject.GetComponent<TextCharacter>();
					yield return textCharacter.Initialize(this, data);
					characters[data.ShortName] = textCharacter;
					break;
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
				bool isVisible = character is GraphicsCharacter graphicsCharacter && graphicsCharacter.IsVisible;
				if (!isVisible)
				{
					characterCount++;
					character.Root.SetSiblingIndex(0);
				}
			}

			return characterCount;
		}

		void CreateDefaultCharacter(string shortName)
		{
			CharacterData data = characterBank.GetDefaultData(shortName, vnOptions);

			GameObject textCharacterGameObject = Instantiate(textCharacterPrefab, characterContainer, false);
			TextCharacter textCharacter = textCharacterGameObject.GetComponent<TextCharacter>();
			textCharacter.InitializeBase(this, data);

			characters[data.ShortName] = textCharacter;
		}

		bool IsValidShortName(string input)
		{
			return Regex.IsMatch(input, @"^[_a-zA-Z][_a-zA-Z0-9]*$");
		}
	}
}
