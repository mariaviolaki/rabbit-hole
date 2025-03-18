using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
	public abstract class Character
	{
		public CharacterManager Manager { get; private set; }
		public CharacterData Data { get; private set; }

		public virtual bool IsChangingVisibility() { return false; }
		public virtual Coroutine Show() { return null; }
		public virtual Coroutine Hide() { return null; }

		public Character(CharacterManager characterManager, CharacterData data)
		{
			Manager = characterManager;
			Data = data;
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
