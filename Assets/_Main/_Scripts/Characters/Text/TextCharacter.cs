using System.Collections;

namespace Characters
{
	public class TextCharacter : Character
	{
		public override IEnumerator Initialize(CharacterManager manager, CharacterData data)
		{
			yield return base.Initialize(manager, data);
		}
	}
}
