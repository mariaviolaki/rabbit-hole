using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
	public abstract class Character
	{
		protected RectTransform root;
		protected CharacterData data;
		protected CharacterManager manager;
		protected bool isVisible = false;

		public RectTransform Root { get { return root; } }
		public CharacterData Data { get { return data; } }
		public bool IsVisible { get { return isVisible; } }

		public static event Action<Character> OnCreateCharacter;

		protected Character() { }
		protected abstract IEnumerator Init();

		public static IEnumerator Create<T>(CharacterManager characterManager, CharacterData data) where T : Character, new()
		{
			T character = new T();
			character.manager = characterManager;
			character.data = data;

			// Perform any asyncronous operations needed for initialization
			yield return character.Init();
			OnCreateCharacter?.Invoke(character);
		}

		public static TextCharacter CreateDefault(CharacterManager characterManager, CharacterData data)
		{
			TextCharacter textCharacter = new TextCharacter();
			textCharacter.manager = characterManager;
			textCharacter.data = data;

			return textCharacter;
		}

		public void ResetData()
		{
			data = manager.Bank.GetCharacterData(Data.Name, Data.CastName, manager.GameOptions);
		}

		public void SetName(string name)
		{
			if (string.IsNullOrEmpty(name)) return;

			data.Name = name;
		}

		public void SetDisplayName(string displayName)
		{
			if (string.IsNullOrEmpty(displayName)) return;

			data.DisplayName = displayName;
		}

		public Coroutine Say(string dialogueLine)
		{
			return Say(new List<string> { dialogueLine });
		}

		public Coroutine Say(List<string> dialogueLines)
		{
			return manager.Dialogue.Say(Data.Name, dialogueLines);
		}
	}
}
