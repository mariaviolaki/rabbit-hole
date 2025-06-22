using Dialogue;
using TMPro;
using UI;
using UnityEngine;

namespace History
{
	[System.Serializable]
	public class HistoryDialogueData
	{
		[SerializeField] string speakerText;
		[SerializeField] string dialogueText;
		[SerializeField] Color speakerColor;
		[SerializeField] Color dialogueColor;
		[SerializeField] string speakerFont;
		[SerializeField] string dialogueFont;

		public HistoryDialogueData(DialogueUI dialogueUI)
		{
			TextMeshProUGUI speakerTextData = dialogueUI.SpeakerText;
			TextMeshProUGUI dialogueTextData = dialogueUI.DialogueText;

			speakerText = speakerTextData.text;
			dialogueText = dialogueTextData.text;
			speakerColor = speakerTextData.color;
			dialogueColor = dialogueTextData.color;
			speakerFont = speakerTextData.font.name;
			dialogueFont = dialogueTextData.font.name;
		}

		public void Apply(DialogueUI dialogueUI, DialogueReader dialogueReader, GameOptionsSO gameOptions, FontBankSO fontBank)
		{
			if (dialogueUI.SpeakerText.text != speakerText)
			{
				// Apply the speaker name and any custom colors and fonts this speaker uses for dialogue
				ApplySpeakerText(dialogueUI, gameOptions, fontBank);
			}
			if (dialogueUI.DialogueText.text != dialogueText)
			{
				// Rewrite the text in the dialogue box
				dialogueReader.ReadDirectText(dialogueText);
			}
		}

		void ApplySpeakerText(DialogueUI dialogueUI, GameOptionsSO gameOptions, FontBankSO fontBank)
		{
			float fadeSpeed = gameOptions.General.SkipTransitionSpeed;

			if (speakerText == string.Empty)
			{
				dialogueUI.HideSpeaker(false, fadeSpeed);
			}
			else
			{
				TMP_FontAsset speakerFontAsset = fontBank.GetFontAsset(speakerFont);
				TMP_FontAsset dialogueFontAsset = fontBank.GetFontAsset(dialogueFont);

				dialogueUI.ShowSpeaker(
					speakerText, speakerFontAsset, speakerColor, dialogueFontAsset, dialogueColor, false, fadeSpeed);
			}
		}
	}
}
