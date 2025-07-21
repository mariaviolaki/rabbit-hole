using IO;
using Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Dialogue
{
	public class DialogueLineBank
	{
		readonly Dictionary<string, int> dialogueLineIds = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<int, string> dialogueLineKeys = new();
		int maxLineId = 0;

		public DialogueLineBank(GameManager gameManager, DialogueManager dialogueManager, FileManagerSO fileManager)
		{
			gameManager.StartCoroutine(RegisterDialogueLines(dialogueManager, fileManager));
		}

		public int GetLineId(string lineKey)
		{
			bool isFound = dialogueLineIds.TryGetValue(lineKey, out int lineId);
			return isFound ? lineId : 0;
		}

		public string GetLineKey(int lineId)
		{
			bool isFound = dialogueLineKeys.TryGetValue(lineId, out string lineKey);
			return isFound ? lineKey : null;
		}

		IEnumerator RegisterDialogueLines(DialogueManager dialogueManager, FileManagerSO fileManager)
		{
			// Wait until all dialogue paths have been loaded
			while (true)
			{
				bool isLogicManagerInitialized = dialogueManager.Logic != null;
				bool arePathsInitialized = fileManager.InitializedKeys.TryGetValue(AssetType.Dialogue, out bool value) && value;
				
				if (isLogicManagerInitialized && arePathsInitialized) break;
				yield return null;
			}

			foreach (string filePath in fileManager.DialogueKeys.Keys)
			{
				yield return fileManager.LoadDialogue(filePath);
				TextAsset dialogueAsset = fileManager.GetDialogueFile(filePath);
				if (dialogueAsset == null) continue;

				ParseLines(dialogueAsset, filePath, dialogueManager.Logic);

				fileManager.UnloadDialogue(filePath);
			}
		}

		void ParseLines(TextAsset dialogueAsset, string filePath, LogicSegmentManager logicSegmentManager)
		{
			using StringReader sr = new StringReader(dialogueAsset.text);
			string line;
			int lineNumber = -1;

			while ((line = sr.ReadLine()) != null)
			{
				if (string.IsNullOrWhiteSpace(line) || line.StartsWith(DialogueFile.CommentLineDelimiter)) continue;

				lineNumber++;
				DialogueLine dialogueLine = DialogueParser.Parse(filePath, lineNumber, line, logicSegmentManager);
				if (dialogueLine == null || dialogueLine.Dialogue == null) continue;

				maxLineId++;
				string lineIdString = DialogueUtilities.GetDialogueLineId(filePath, lineNumber);
				dialogueLineIds[lineIdString] = maxLineId;
				dialogueLineKeys[maxLineId] = lineIdString;
			}
		}
	}
}
