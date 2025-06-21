using Dialogue;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Logic
{
	public class ConditionLogicSegment : NonBlockingLogicSegmentBase
	{
		const string ifKeyword = "if";
		const string elseKeyword = "else";
		const char ConditionStart = '(';
		const char ConditionEnd = ')';
		public static new bool Matches(string rawLine) => StartsWithKeyword(rawLine, ifKeyword);

		readonly DialogueStack dialogueStack;
		readonly LogicSegmentUtils logicSegmentUtils;

		public ConditionLogicSegment(DialogueSystem dialogueSystem, string rawLine) : base(dialogueSystem, rawLine)
		{
			dialogueStack = dialogueSystem.Reader.Stack;
			logicSegmentUtils = new(dialogueSystem);
		}

		public override void Execute()
		{
			ParseConditions();
		}

		void ParseConditions()
		{
			DialogueBlock dialogueBlock = dialogueStack.GetBlock();
			if (dialogueBlock == null)
			{
				Debug.LogWarning("No Dialogue Block found in stack while parsing conditions.");
				return;
			}

			List<LogicCondition> logicConditions = ParseConditionBlocks(dialogueBlock);
			if (logicConditions.Count == 0) return;

			LogicCondition lastCondition = logicConditions.Last();
			if (lastCondition.Block.EndIndex == -1) return;

			dialogueBlock.SetProgress(lastCondition.Block.EndIndex);

			foreach (LogicCondition logicCondition in logicConditions)
			{
				// This is likely an else block
				if (string.IsNullOrWhiteSpace(logicCondition.Condition) && logicCondition.Block.Lines.Count > 0)
				{
					dialogueStack.AddBlock(logicCondition.Block.Lines);
					break;
				}

				// Check if the condition is successful in an if block
				string resultString = logicSegmentUtils.EvaluateExpression(logicCondition.Condition);
				if (!bool.TryParse(resultString, out bool resultBool) || !resultBool) continue;

				dialogueStack.AddBlock(logicCondition.Block.Lines);
				break;
			}
		}

		List<LogicCondition> ParseConditionBlocks(DialogueBlock dialogueBlock)
		{
			List<string> lines = dialogueBlock.Lines;
			List<LogicCondition> logicConditions = new();
			int progress = dialogueBlock.Progress;

			while (progress < lines.Count)
			{
				// Ignore any whitespace between blocks
				string line = lines[progress].Trim();
				if (string.IsNullOrWhiteSpace(line))
				{
					progress++;
					continue;
				}

				// Only allow valid keywords and whitespace between blocks
				string condition = null;
				bool isIfCondition = line.StartsWith(ifKeyword);
				bool isElseCondition = line.StartsWith(elseKeyword);

				if (!isIfCondition && !isElseCondition) break;

				// If there is a condition between parentheses parse it and save it for the current block
				int conditionStart = line.IndexOf(ConditionStart);
				int conditionEnd = line.IndexOf(ConditionEnd);
				if (conditionStart >= 0 && conditionEnd > conditionStart)
					condition = line.Substring(conditionStart + 1, conditionEnd - conditionStart - 1).Trim();

				LogicBlock logicBlock = logicSegmentUtils.ParseBlock(dialogueBlock, progress);
				if (logicBlock == null) break;

				// Stop parsing if this is an else block without condition
				logicConditions.Add(new LogicCondition(condition, logicBlock));
				if (isElseCondition && condition == null) break;

				progress = logicBlock.EndIndex + 1;
			}

			return logicConditions;
		}
	}
}
