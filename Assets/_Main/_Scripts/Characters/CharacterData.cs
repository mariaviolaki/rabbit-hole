using TMPro;
using UnityEngine;

namespace Characters
{
	[System.Serializable]
	public class CharacterData
	{
		[SerializeField] string name;
		[SerializeField] string alias;
		[SerializeField] Color nameColor;
		[SerializeField] Color dialogueColor;
		[SerializeField] TMP_FontAsset nameFont;
		[SerializeField] TMP_FontAsset dialogueFont;
		[SerializeField] CharacterType type;

		public string Name { get { return name; } set { name = value; } }
		public string Alias { get { return alias; } set { alias = value; } }
		public Color NameColor { get { return nameColor; } set { nameColor = value; } }
		public Color DialogueColor { get { return dialogueColor; } set { dialogueColor = value; } }
		public TMP_FontAsset NameFont { get { return nameFont; } set { nameFont = value; } }
		public TMP_FontAsset DialogueFont { get { return dialogueFont; } set { dialogueFont = value; } }
		public CharacterType Type { get { return type; } }

		public static CharacterData GetDefault(string name, GameOptionsSO gameOptions)
		{
			return new CharacterData(name, gameOptions);
		}

		public CharacterData Copy()
		{
			return new CharacterData(this);
		}

		// Used to copy valid character data
		CharacterData(CharacterData originalData)
		{
			name = originalData.name;
			alias = originalData.alias;
			type = originalData.type;
			nameColor = originalData.nameColor;
			dialogueColor = originalData.dialogueColor;
			nameFont = originalData.nameFont;
			dialogueFont = originalData.dialogueFont;
		}

		// Used to generate default character data
		CharacterData(string name, GameOptionsSO gameOptions)
		{
			this.name = name;
			alias = name;
			type = CharacterType.Text;
			nameColor = gameOptions.DefaultTextColor;
			dialogueColor = gameOptions.DefaultTextColor;
			nameFont = gameOptions.DefaultFont;
			dialogueFont = gameOptions.DefaultFont;
		}
	}
}
