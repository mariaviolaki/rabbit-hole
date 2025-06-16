using Characters;
using Dialogue;
using TMPro;
using UnityEngine;

namespace UI
{
	public class DialogueUI : FadeableUI
	{
		[SerializeField] FontDirectorySO fontDirectory;
		[SerializeField] DialogueNameUI dialogueNameUI;
		[SerializeField] TextMeshProUGUI dialogueText;

		public TextMeshProUGUI DialogueText => dialogueText;

		protected override void Start()
		{
			UpdateFontSize(gameOptions.Dialogue.DefaultFont);
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
			UpdateDialogueText(null);

			return dialogueNameUI.HideSpeaker(isImmediate, fadeSpeed);
		}

		void UpdateDialogueText(CharacterData characterData)
		{
			if (characterData == null)
			{
				dialogueText.color = gameOptions.Dialogue.DefaultTextColor;
				dialogueText.font = gameOptions.Dialogue.DefaultFont;
				UpdateFontSize(gameOptions.Dialogue.DefaultFont);
			}
			else
			{
				dialogueText.color = characterData.DialogueColor;
				dialogueText.font = characterData.DialogueFont;
				UpdateFontSize(characterData.DialogueFont);
			}
		}

		void UpdateFontSize(TMP_FontAsset font)
		{
			float baseFontSize = gameOptions.Dialogue.DialogueFontSize;

			float fontSizeMultiplier = 1f;
			if (fontDirectory.Fonts.TryGetValue(font.name, out FontData fontData))
				fontSizeMultiplier = fontData.SizeMultiplier;

			dialogueText.fontSize = baseFontSize * fontSizeMultiplier;
		}
	}
}
