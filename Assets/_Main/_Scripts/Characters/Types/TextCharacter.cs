using UnityEngine;

namespace Characters
{
	public class TextCharacter : Character
	{
		public TextCharacter(CharacterManager characterManager, string name) : base(characterManager, name)
		{
			Debug.Log($"Created Text Character: {name}");
		}
	}
}
