using Characters;
using TMPro;
using UnityEngine;

namespace UI
{
	public class DialogueNameUI : FadeableUI
	{
		[SerializeField] TextMeshProUGUI nameText;

		public TextMeshProUGUI NameText => nameText;

		public Coroutine ShowSpeaker(string speakerName, TMP_FontAsset font, Color color, bool isImmediate = false, float fadeSpeed = 0)
		{
			UpdateNameText(speakerName, font, color);
			return Show(isImmediate, fadeSpeed);
		}

		public Coroutine ShowSpeaker(CharacterData characterData, bool isImmediate = false, float fadeSpeed = 0)
		{
			UpdateNameText(characterData);
			return Show(isImmediate, fadeSpeed);
		}

		public Coroutine HideSpeaker(bool isImmediate = false, float fadeSpeed = 0)
		{
			UpdateNameText();
			return Hide(isImmediate, fadeSpeed);
		}

		void UpdateNameText() => UpdateNameText("", gameOptions.Dialogue.DefaultFont, gameOptions.Dialogue.DefaultTextColor);
		void UpdateNameText(CharacterData characterData) => UpdateNameText(characterData?.DisplayName, characterData?.NameFont, characterData.NameColor);
		void UpdateNameText(string speakerName, TMP_FontAsset font, Color color)
		{
			if (font == null)
				font = gameOptions.Dialogue.DefaultFont;

			nameText.text = speakerName;
			nameText.color = color;
			nameText.font = font;
		}
	}
}
