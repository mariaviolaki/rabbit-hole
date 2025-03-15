using UnityEngine;

namespace Characters
{
	public class SpriteCharacter : Character
	{
		public SpriteCharacter(string name, CharacterManager characterManager) : base(name, characterManager)
		{
			Debug.Log("Created Sprite Character");
		}
	}
}
