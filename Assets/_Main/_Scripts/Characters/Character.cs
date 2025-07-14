using System;
using System.Collections;
using UnityEngine;

namespace Characters
{
	public abstract class Character
	{
		protected CharacterManager manager;
		protected RectTransform root;
		protected CharacterData data;
		protected bool isVisible = false;

		public CharacterManager Manager => manager;
		public RectTransform Root => root;
		public CharacterData Data => data;
		public bool IsVisible => isVisible;

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
	}
}
