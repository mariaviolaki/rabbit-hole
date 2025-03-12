using UnityEngine;

namespace Characters
{
	public class SpriteCharacter : Character
	{
		public SpriteCharacter(string name, CharacterDirectorySO directory, GameOptionsSO options)
			: base(name, directory, options)
		{
			Debug.Log("Created Sprite Character");
		}
	}
}
