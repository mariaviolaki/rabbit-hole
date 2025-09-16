using Characters;
using Dialogue;
using System.Collections;
using System.Collections.Generic;
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

		void Start()
		{
			UpdateFontSize(vnOptions.Dialogue.DefaultFont);
		}

		public IEnumerator ShowSpeaker(CharacterData characterData, bool isImmediate = false, float fadeSpeed = 0)
		{
			UpdateDialogueText(characterData);

			// Ensure that the dialogue box is visible when a character speaks
			List<IEnumerator> processes = new()
			{
				dialogueNameUI.ShowSpeaker(characterData, isImmediate, fadeSpeed),
				SetVisible(isImmediate, fadeSpeed)
			};

			yield return Utilities.RunConcurrentProcesses(this, processes);
		}

		public IEnumerator HideSpeaker(bool isImmediate = false, float fadeSpeed = 0)
		{
			// When there is no speaker specified, revert to default text as dialogue
			UpdateDialogueText();

			// Ensure that the dialogue box is visible when a character speaks
			List<IEnumerator> processes = new()
			{
				dialogueNameUI.HideSpeaker(isImmediate, fadeSpeed),
				SetVisible(isImmediate, fadeSpeed)
			};

			yield return Utilities.RunConcurrentProcesses(this, processes);
		}

		void UpdateDialogueText() => UpdateDialogueText(vnOptions.Dialogue.DefaultTextColor);
		void UpdateDialogueText(CharacterData characterData) => UpdateDialogueText(characterData.DialogueColor);
		void UpdateDialogueText(Color color)
		{
			dialogueText.color = color;
		}

		void UpdateFontSize(TMP_FontAsset font)
		{
			float baseFontSize = vnOptions.Dialogue.DialogueFontSize;

			float fontSizeMultiplier = 1f;
			if (fontBank.Fonts.TryGetValue(font.name, out FontData fontData))
				fontSizeMultiplier = fontData.SizeMultiplier;

			dialogueText.fontSize = baseFontSize * fontSizeMultiplier;
		}
	}
}
