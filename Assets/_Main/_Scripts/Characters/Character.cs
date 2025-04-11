using System.Collections.Generic;
using System.Threading.Tasks;
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

		protected Character() { }
		protected abstract Task Init();

		public static async Task<T> Create<T>(CharacterManager characterManager, CharacterData data) where T : Character, new()
		{
			T character = new T();
			character.manager = characterManager;
			character.data = data;

			// Perform any asyncronous operations needed for initialization
			await character.Init();

			return character;
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
			data = manager.Directory.GetCharacterData(Data.Name, Data.CastName, manager.GameOptions);
		}

		public Coroutine Say(string dialogueLine)
		{
			return manager.Dialogue.Say(Data.Name, dialogueLine);
		}

		public Coroutine Say(List<string> dialogueLines)
		{
			return manager.Dialogue.Say(Data.Name, dialogueLines);
		}
	}
}
