using TMPro;
using UnityEngine;

namespace Characters
{
	[System.Serializable]
	public class CharacterData
	{
		public string name;
		public string alias;
		public CharacterType type;
		public Color nameColor;
		public Color dialogueColor;
		public TMP_FontAsset nameFont;
		public TMP_FontAsset dialogueFont;

		public static CharacterData GetDefault(GameOptionsSO gameOptions)
		{
			return new CharacterData(gameOptions);
		}

		CharacterData(GameOptionsSO gameOptions)
		{
			name = "";
			alias = "";
			type = CharacterType.Text;
			nameColor = gameOptions.DefaultTextColor;
			dialogueColor = gameOptions.DefaultTextColor;
			nameFont = gameOptions.DefaultFont;
			dialogueFont = gameOptions.DefaultFont;
		}
	}
}
