using Dialogue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace Logic
{
	public class ChoiceLogicSegment : BlockingLogicSegmentBase
	{
		const string keyword = "choice";
		public static new bool Matches(string rawLine) => StartsWithKeyword(rawLine, keyword);

		const char ChoiceStartDelimiter = '-';

		readonly InputManagerSO inputManager;
		readonly VisualNovelUI visualNovelUI;
		readonly DialogueStack dialogueStack;
		List<DialogueChoice> choices = new();
		DialogueChoice choice;
		LogicSegmentUtils logicSegmentUtils;

		public ChoiceLogicSegment(DialogueSystem dialogueSystem, string rawLine) : base(dialogueSystem, rawLine)
		{
			inputManager = dialogueSystem.InputManager;
			visualNovelUI = dialogueSystem.UI;
			dialogueStack = dialogueSystem.Reader.Stack;
			logicSegmentUtils = new(dialogueSystem);

			ParseChoices();
		}

		public override IEnumerator Execute()
		{
			if (choices.Count == 0) yield break;

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

			LogicBlock logicBlock = logicSegmentUtils.ParseBlock(dialogueBlock, dialogueBlock.Progress);
			if (logicBlock == null) return;

			int depth = 0; // start outside of any blocks

			foreach (string line in logicBlock.Lines)
			{
				if (line.StartsWith(LogicSegmentUtils.BlockStartDelimiter))
					depth++;
				else if (line.StartsWith(LogicSegmentUtils.BlockEndDelimiter))
					depth--;

				if (line.StartsWith(ChoiceStartDelimiter) && depth == 0)
				{
					string choiceText = line.Substring(1).Trim();
					choices.Add(new DialogueChoice(choices.Count, choiceText));
					continue;
				}

				if (choices.Count > 0)
					choices.Last().DialogueLines.Add(line);
			}

			// Continue the main dialogue after the choice block
			dialogueBlock.SetProgress(logicBlock.EndIndex);
		}
	}
}
