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

		public string CastName { get; set; } // to use another character's data
		public string Name { get { return name; } set { name = value; } }
		public string Alias { get { return alias; } set { alias = value; } }
		public Color NameColor { get { return nameColor; } set { nameColor = value; } }
		public Color DialogueColor { get { return dialogueColor; } set { dialogueColor = value; } }
		public TMP_FontAsset NameFont { get { return nameFont; } set { nameFont = value; } }
		public TMP_FontAsset DialogueFont { get { return dialogueFont; } set { dialogueFont = value; } }
		public CharacterType Type { get { return type; } private set { type = value; } }

		public static CharacterData Get(string name, CharacterData originalData)
		{
			CharacterData copiedData = new CharacterData();
			copiedData.CastName = originalData.name;
			copiedData.Name = name;
			copiedData.Alias = name;
			copiedData.Type = originalData.Type;
			copiedData.NameColor = originalData.nameColor;
			copiedData.DialogueColor = originalData.dialogueColor;
			copiedData.NameFont = originalData.nameFont;
			copiedData.DialogueFont = originalData.dialogueFont;

			return copiedData;
		}

		public static CharacterData GetDefault(string name, GameOptionsSO gameOptions)
		{
			CharacterData defaultData = new CharacterData();
			defaultData.CastName = name;
			defaultData.Name = name;
			defaultData.Alias = name;
			defaultData.Type = CharacterType.Text;
			defaultData.NameColor = gameOptions.DefaultTextColor;
			defaultData.DialogueColor = gameOptions.DefaultTextColor;
			defaultData.NameFont = gameOptions.DefaultFont;
			defaultData.DialogueFont = gameOptions.DefaultFont;

			return defaultData;
		}
	}
}
