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
		[SerializeField] int dialogueLineId;
		[SerializeField] string speakerText;
		[SerializeField] string dialogueText;
		[SerializeField] Color speakerColor;
		[SerializeField] Color dialogueColor;
		[SerializeField] string speakerFont;
		[SerializeField] string dialogueFont;

		public int DialogueLineId => dialogueLineId;
		public string SpeakerText => speakerText;
		public string DialogueText => dialogueText;
		
		public HistoryDialogueData(DialogueStack dialogueStack, DialogueLineBank dialogueLineBank, DialogueUI dialogueUI)
		{
			TextMeshProUGUI speakerTextData = dialogueUI.SpeakerText;
			TextMeshProUGUI dialogueTextData = dialogueUI.DialogueText;

			speakerText = speakerTextData.text;
			dialogueText = dialogueTextData.text;
			speakerColor = speakerTextData.color;
			dialogueColor = dialogueTextData.color;
			speakerFont = speakerTextData.font.name;
			dialogueFont = dialogueTextData.font.name;

			DialogueBlock lastDialogueBlock = dialogueStack.Blocks.Peek();
			foreach (DialogueBlock dialogueBlock in dialogueStack.Blocks)
			{
				if (dialogueBlock == lastDialogueBlock)
				{
					string dialogueLineKey = DialogueUtilities.GetDialogueLineId(dialogueBlock.FilePath, dialogueBlock.Progress);
					dialogueLineId = dialogueLineBank.GetLineId(dialogueLineKey);
				}

				HistoryDialogueBlock historyBlock = new();
				historyBlock.filePath = dialogueBlock.FilePath;
				historyBlock.fileStartIndex = dialogueBlock.FileStartIndex;
				historyBlock.fileEndIndex = dialogueBlock.FileEndIndex;
				historyBlock.progress = dialogueBlock.Progress;

				dialogueBlocks.Add(historyBlock);
			}
		}

		public IEnumerator Load(DialogueUI dialogueUI, DialogueReader dialogueReader, GameOptionsSO gameOptions, FontBankSO fontBank)
		{
			if (dialogueUI.SpeakerText.text != speakerText)
			{
				// Apply the speaker name and any custom colors and fonts this speaker uses for dialogue
				UpdateSpeakerText(dialogueUI, gameOptions, fontBank);
			}
			if (dialogueUI.DialogueText.text != dialogueText)
			{
				// Rewrite the text in the dialogue box
				yield return dialogueReader.ReadImmediate(dialogueText);
			}
		}

		public void Apply(HistoryState historyState, DialogueManager dialogueManager)
		{
			if (dialogueBlocks.Count == 0) return;

			// The dialogue manager will now end the current reading process
			HistoryDialogueBlock mainHistoryBlock = dialogueBlocks.Last();
			// The restore function will be called shortly for this history state, after dialogue manager loads the new dialogue file
			dialogueManager.LoadDialogueWithProgress(mainHistoryBlock.filePath, historyState);
		}

		public void RestoreDialogueProgress(DialogueStack dialogueStack)
		{
			// The dialogue manager just loaded the main file so the stack should only contain its main block
			DialogueBlock mainBlock = dialogueStack.GetBlock();
			if (mainBlock == null) return;

			HistoryDialogueBlock mainHistoryBlock = dialogueBlocks.Last();
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
				dialogueStack.AddBlock(
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
