using UnityEngine;

namespace Characters
{
	public class TextCharacter : Character
	{
		public TextCharacter(string name, CharacterManager characterManager) : base(name, characterManager)
		{
			Debug.Log("Created Text Character");
		}
	}
}
