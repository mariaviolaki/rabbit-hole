using Dialogue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;

namespace History
{
	[System.Serializable]
	public class HistoryDialogueData
	{
		[SerializeField] List<HistoryDialogueBlock> dialogueBlocks = new();
		[SerializeField] string speakerText;
		[SerializeField] string dialogueText;
		[SerializeField] Color speakerColor;
		[SerializeField] Color dialogueColor;
		[SerializeField] string speakerFont;
		[SerializeField] string dialogueFont;

		public HistoryDialogueData(DialogueStack dialogueStack, DialogueUI dialogueUI)
		{
			TextMeshProUGUI speakerTextData = dialogueUI.SpeakerText;
			TextMeshProUGUI dialogueTextData = dialogueUI.DialogueText;

			speakerText = speakerTextData.text;
			dialogueText = dialogueTextData.text;
			speakerColor = speakerTextData.color;
			dialogueColor = dialogueTextData.color;
			speakerFont = speakerTextData.font.name;
			dialogueFont = dialogueTextData.font.name;

			foreach (DialogueBlock dialogueBlock in dialogueStack.Blocks)
			{
				HistoryDialogueBlock historyBlock = new();
				historyBlock.filePath = dialogueBlock.FilePath;
				historyBlock.fileStartIndex = dialogueBlock.FileStartIndex;
				historyBlock.fileEndIndex = dialogueBlock.FileEndIndex;
				historyBlock.progress = dialogueBlock.Progress;

				dialogueBlocks.Add(historyBlock);
			}
		}

		public void Load(DialogueUI dialogueUI, DialogueReader dialogueReader, GameOptionsSO gameOptions, FontBankSO fontBank)
		{
			if (dialogueUI.SpeakerText.text != speakerText)
			{
				// Apply the speaker name and any custom colors and fonts this speaker uses for dialogue
				UpdateSpeakerText(dialogueUI, gameOptions, fontBank);
			}
			if (dialogueUI.DialogueText.text != dialogueText)
			{
				// Rewrite the text in the dialogue box
				dialogueReader.ReadDirectText(dialogueText);
			}
		}

		public IEnumerator Apply(DialogueSystem dialogueSystem)
		{
			if (dialogueBlocks.Count == 0) yield break;

			dialogueSystem.StopDialogue();

			// Load the file and add its parsed contents to the bottom of the stack
			HistoryDialogueBlock mainHistoryBlock = dialogueBlocks.Last();
			yield return dialogueSystem.LoadDialogue(mainHistoryBlock.filePath);

			// Move the progress of the main block to the right point in the history state
			DialogueBlock mainBlock = dialogueSystem.Reader.Stack.GetBlock();
			mainBlock.LoadProgress(mainHistoryBlock.progress, mainHistoryBlock.fileStartIndex, mainHistoryBlock.fileEndIndex);

			for (int i = dialogueBlocks.Count - 2; i >= 0; i--)
			{
				HistoryDialogueBlock historyBlock = dialogueBlocks[i];

				// Get a subset of the lines from the main block based on the history block's start and end indices
				List<string> lines = mainBlock.Lines
					.Skip(historyBlock.fileStartIndex)
					.Take(historyBlock.fileEndIndex - historyBlock.fileStartIndex + 1)
					.ToList();

				// Add any subsequent nested blocks back to the stack
				dialogueSystem.Reader.Stack.AddBlock(
					historyBlock.filePath, lines, historyBlock.fileStartIndex, historyBlock.fileEndIndex, historyBlock.progress);
			}
		}

		void UpdateSpeakerText(DialogueUI dialogueUI, GameOptionsSO gameOptions, FontBankSO fontBank)
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
