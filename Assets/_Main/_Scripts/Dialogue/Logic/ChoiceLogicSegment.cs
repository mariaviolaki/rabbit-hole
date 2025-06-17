using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace Dialogue
{
	public class ChoiceLogicSegment : LogicSegmentBase
	{
		public static new string Keyword => "Choice";

		const char BlockStartDelimiter = '{';
		const char BlockEndDelimiter = '}';
		const char ChoiceStartDelimiter = '-';

		readonly InputManagerSO inputManager;
		readonly VisualNovelUI visualNovelUI;
		readonly DialogueStack dialogueStack;
		List<DialogueChoice> choices = new();
		DialogueChoice choice;

		public ChoiceLogicSegment(DialogueSystem dialogueSystem, string rawData) : base(dialogueSystem, rawData)
		{
			inputManager = dialogueSystem.InputManager;
			visualNovelUI = dialogueSystem.UI;
			dialogueStack = dialogueSystem.Reader.Stack;

			ParseChoices();
		}

		public override IEnumerator Execute()
		{
			inputManager.OnClearChoice += HandleOnClearChoiceEvent;
			inputManager.OnSelectChoice += HandleOnSelectChoiceEvent;

			try
			{
				yield return visualNovelUI.ShowChoices(choices);
				while (choice == null) yield return null;
				dialogueStack.AddBlock(choice.DialogueLines);
			}
			finally
			{
				inputManager.OnClearChoice -= HandleOnClearChoiceEvent;
				inputManager.OnSelectChoice -= HandleOnSelectChoiceEvent;
			}
		}

		void HandleOnClearChoiceEvent() => HandleChoiceEvent(null);
		void HandleOnSelectChoiceEvent(DialogueChoice choice) => HandleChoiceEvent(choice);
		void HandleChoiceEvent(DialogueChoice choice) => this.choice = choice;

		void ParseChoices()
		{
			DialogueBlock dialogueBlock = dialogueStack.GetBlock();
			if (dialogueBlock == null)
			{
				Debug.LogWarning("No Dialogue Block found in stack while parsing choices.");
				return;
			}

			List<string> rawLines = dialogueBlock.Lines;
			int progress = dialogueBlock.Progress + 1;
			int depth = -1; // start outside of any blocks

			while (progress < rawLines.Count)
			{
				string rawLine = rawLines[progress].TrimStart();
				bool isLineSkipped = false;

				if (rawLine.StartsWith(ChoiceStartDelimiter) && depth == 0)
				{
					string choiceText = rawLine.Substring(1).Trim();
					choices.Add(new DialogueChoice(choices.Count, choiceText));
					isLineSkipped = true;
				}
				else if (rawLine.StartsWith(BlockStartDelimiter))
				{
					depth++;
					isLineSkipped = depth == 0;
				}
				else if (rawLine.StartsWith(BlockEndDelimiter))
				{
					depth--;
					if (depth < 0) break;
				}

				if (choices.Count > 0 && !isLineSkipped)
					choices.Last().DialogueLines.Add(rawLines[progress]);

				progress++;
			}

			// Continue the main dialogue after the choice block
			dialogueBlock.SetProgress(progress - 1);
		}
	}
}
