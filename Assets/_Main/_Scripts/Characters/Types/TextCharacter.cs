using UnityEngine;

namespace Characters
{
	public class TextCharacter : Character
	{
		public TextCharacter(CharacterManager characterManager, CharacterData data)
			: base(characterManager, data)
		{
			Debug.Log($"Created Text Character: {data.Name}");
		}
	}
}
