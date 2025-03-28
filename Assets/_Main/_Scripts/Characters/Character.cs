using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Characters
{
	public abstract class Character
	{
		public CharacterManager Manager { get; protected set; }
		public CharacterData Data { get; protected set; }
		public RectTransform Root { get; protected set; }
		public bool IsVisible { get; protected set; } = false;

		public virtual bool IsChangingVisibility() { return false; }
		public virtual Coroutine Show() { return null; }
		public virtual Coroutine Hide() { return null; }
		public virtual Coroutine MoveToPosition(Vector2 position, float speed) { return null; }
		public virtual void SetPosition(Vector2 position) { }

		protected Character() { }
		protected abstract Task Init();

		public static async Task<T> Create<T>(CharacterManager characterManager, CharacterData data) where T : Character, new()
		{
			T character = new T();
			character.Manager = characterManager;
			character.Data = data;

			// Perform any asyncronous operations needed for initialization
			await character.Init();

			return character;
		}

		public static TextCharacter CreateDefault(CharacterManager characterManager, CharacterData data)
		{
			TextCharacter textCharacter = new TextCharacter();
			textCharacter.Manager = characterManager;
			textCharacter.Data = data;

			return textCharacter;
		}

		public void ResetData()
		{
			Data = Manager.Directory.GetCharacterData(Data.Name, Data.CastName, Manager.GameOptions);
		}

		public Coroutine Say(string dialogueLine)
		{
			return Manager.Dialogue.Say(Data.Name, dialogueLine);
		}

		public Coroutine Say(List<string> dialogueLines)
		{
			return Manager.Dialogue.Say(Data.Name, dialogueLines);
		}
	}
}
