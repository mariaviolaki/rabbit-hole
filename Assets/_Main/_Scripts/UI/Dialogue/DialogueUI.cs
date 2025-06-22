using Characters;
using Dialogue;
using TMPro;
using UnityEngine;

namespace UI
{
	public class DialogueUI : FadeableUI
	{
		[SerializeField] FontBankSO fontBank;
		[SerializeField] DialogueNameUI dialogueNameUI;
		[SerializeField] TextMeshProUGUI dialogueText;

		public TextMeshProUGUI SpeakerText => dialogueNameUI.NameText;
		public TextMeshProUGUI DialogueText => dialogueText;

		protected override void Start()
		{
			UpdateFontSize(gameOptions.Dialogue.DefaultFont);
		}

		public Coroutine ShowSpeaker(string speakerName, TMP_FontAsset speakerFont, Color speakerColor,
			TMP_FontAsset dialogueFont, Color dialogueColor, bool isImmediate = false, float fadeSpeed = 0)
		{
			UpdateDialogueText(dialogueFont, dialogueColor);
			dialogueNameUI.ShowSpeaker(speakerName, speakerFont, speakerColor, isImmediate, fadeSpeed);

			// Ensure that the dialogue box is visible when a character speaks
			return Show(isImmediate, fadeSpeed);
		}

		public Coroutine ShowSpeaker(CharacterData characterData, bool isImmediate = false, float fadeSpeed = 0)
		{
			UpdateDialogueText(characterData);
			dialogueNameUI.ShowSpeaker(characterData, isImmediate, fadeSpeed);

			// Ensure that the dialogue box is visible when a character speaks
			return Show(isImmediate, fadeSpeed);
		}

		public Coroutine HideSpeaker(bool isImmediate = false, float fadeSpeed = 0)
		{
			// When there is no speaker specified, revert to default text as dialogue
			UpdateDialogueText();

			return dialogueNameUI.HideSpeaker(isImmediate, fadeSpeed);
		}

		void UpdateDialogueText() => UpdateDialogueText(gameOptions.Dialogue.DefaultFont, gameOptions.Dialogue.DefaultTextColor);
		void UpdateDialogueText(CharacterData characterData) => UpdateDialogueText(characterData?.DialogueFont, characterData.DialogueColor);
		void UpdateDialogueText(TMP_FontAsset font, Color color)
		{
			if (font == null)
				font = gameOptions.Dialogue.DefaultFont;

			dialogueText.color = color;
			dialogueText.font = font;
			UpdateFontSize(font);
		}

		void UpdateFontSize(TMP_FontAsset font)
		{
			float baseFontSize = gameOptions.Dialogue.DialogueFontSize;

			float fontSizeMultiplier = 1f;
			if (fontBank.Fonts.TryGetValue(font.name, out FontData fontData))
				fontSizeMultiplier = fontData.SizeMultiplier;

			dialogueText.fontSize = baseFontSize * fontSizeMultiplier;
		}
	}
}
