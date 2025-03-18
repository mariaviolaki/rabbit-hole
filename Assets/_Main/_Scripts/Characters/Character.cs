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

		public Character(CharacterManager characterManager, string name)
		{
			Manager = characterManager;
			Data = Manager.Directory.GetCharacterData(name, Manager.GameOptions);
		}

		public void ResetData()
		{
			Data = Manager.Directory.GetCharacterData(Data.Name, Manager.GameOptions);
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
