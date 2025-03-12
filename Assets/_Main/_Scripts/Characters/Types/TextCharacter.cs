using UnityEngine;

namespace Characters
{
	public class TextCharacter : Character
	{
		public TextCharacter(string name, CharacterDirectorySO directory, GameOptionsSO options)
			: base(name, directory, options)
		{
			Debug.Log("Created Text Character");
		}
	}
}
