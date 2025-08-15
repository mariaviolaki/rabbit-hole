using Characters;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
	public class DialogueNameUI : FadeableUI
	{
		[SerializeField] TextMeshProUGUI nameText;

		public TextMeshProUGUI NameText => nameText;

		public IEnumerator ShowSpeaker(CharacterData characterData, bool isImmediate = false, float fadeSpeed = 0)
		{
			UpdateNameText(characterData);
			yield return SetVisible(isImmediate, fadeSpeed);
		}

		public IEnumerator HideSpeaker(bool isImmediate = false, float fadeSpeed = 0)
		{
			UpdateNameText();
			yield return SetHidden(isImmediate, fadeSpeed);
		}

		void UpdateNameText() => UpdateNameText("", gameOptions.Dialogue.DefaultFont, gameOptions.Dialogue.DefaultTextColor);
		void UpdateNameText(CharacterData characterData) => UpdateNameText(characterData?.Name, characterData?.NameFont, characterData.NameColor);
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
