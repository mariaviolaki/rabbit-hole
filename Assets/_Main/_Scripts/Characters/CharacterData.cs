using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Characters
{
	[System.Serializable]
	public class CharacterSpriteLayerData
	{
		public SpriteLayerType LayerType;
		public Sprite DefaultSprite;
	}

	[System.Serializable]
	public class CharacterData
	{
		[SerializeField] string name;
		[SerializeField] string shortName;
		[SerializeField] Color nameColor;
		[SerializeField] Color dialogueColor;
		[SerializeField] TMP_FontAsset nameFont;
		[SerializeField] TMP_FontAsset dialogueFont;
		[SerializeField] CharacterType type;
		[SerializeField] List<CharacterSpriteLayerData> spriteLayers;

		public string CastName { get; set; } // to use another character's data
		public string Name { get { return name; } set { name = value; } }
		public string ShortName { get { return shortName; } set { shortName = value; } }
		public Color NameColor { get { return nameColor; } set { nameColor = value; } }
		public Color DialogueColor { get { return dialogueColor; } set { dialogueColor = value; } }
		public TMP_FontAsset NameFont { get { return nameFont; } set { nameFont = value; } }
		public TMP_FontAsset DialogueFont { get { return dialogueFont; } set { dialogueFont = value; } }
		public CharacterType Type { get { return type; } private set { type = value; } }
		public List<CharacterSpriteLayerData> SpriteLayers { get { return spriteLayers; } private set { spriteLayers = value; } }

		public static CharacterData Get(string shortName, CharacterData originalData, bool isOriginalCharacter)
		{
			CharacterData copiedData = new();
			copiedData.CastName = originalData.name;
			copiedData.Name = originalData.name;
			copiedData.ShortName = shortName;
			copiedData.Type = originalData.Type;
			copiedData.SpriteLayers = originalData.SpriteLayers;
			copiedData.NameColor = originalData.nameColor;
			copiedData.DialogueColor = originalData.dialogueColor;
			copiedData.NameFont = originalData.nameFont;
			copiedData.DialogueFont = originalData.dialogueFont;

			return copiedData;
		}

		public static CharacterData GetDefault(string shortName, GameOptionsSO gameOptions)
		{
			CharacterData defaultData = new();
			defaultData.CastName = shortName;
			defaultData.Name = shortName;
			defaultData.ShortName = shortName;
			defaultData.Type = CharacterType.Text;
			defaultData.SpriteLayers = new();
			defaultData.NameColor = gameOptions.Dialogue.DefaultTextColor;
			defaultData.DialogueColor = gameOptions.Dialogue.DefaultTextColor;
			defaultData.NameFont = gameOptions.Dialogue.DefaultFont;
			defaultData.DialogueFont = gameOptions.Dialogue.DefaultFont;

			return defaultData;
		}
	}
}
