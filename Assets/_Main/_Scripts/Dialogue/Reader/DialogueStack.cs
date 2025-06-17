using System.Collections.Generic;

namespace Dialogue
{
	public class DialogueStack
	{
		const string CommentLineDelimiter = "//";

		readonly Stack<DialogueBlock> dialogueBlocks = new();

		public bool IsEmpty => dialogueBlocks.Count == 0;

		public void Clear() => dialogueBlocks.Clear();

		public void AddBlock(List<string> lines, int progress = 0)
		{
			DialogueBlock dialogueBlock = new(lines, progress);
			dialogueBlocks.Push(dialogueBlock);
		}

		public string GetNextLine()
		{
			DialogueBlock dialogueBlock = GetBlock();
			if (dialogueBlock == null) return null;

			string rawLine = dialogueBlock.GetLine();

			while (!IsEmpty && !IsValidLine(rawLine))
			{
				Proceed();
				dialogueBlock = GetBlock();
				rawLine = dialogueBlock?.GetLine();
			}

			return IsValidLine(rawLine) ? rawLine : null;
		}

		// Try to get the most recent dialogue block added to the stack
		public DialogueBlock GetBlock()
		{
			dialogueBlocks.TryPeek(out DialogueBlock dialogueBlock);
			return dialogueBlock;
		}

		public bool ProceedInBlock(DialogueBlock dialogueBlock)
		{
			if (dialogueBlock == null) return false;

			return dialogueBlock.IncrementProgress();
		}

		// Try to increment the progress on the most recent dialogue block block
		bool Proceed()
		{
			while (!IsEmpty)
			{
				DialogueBlock dialogueBlock = GetBlock();
				if (dialogueBlock == null) return false;

				bool isIncremented = dialogueBlock.IncrementProgress();
				if (isIncremented) return true;

				// If the progress could not be incremented on this block, assume that it's complete
				Pop();
			}

			return false;
		}

		// Try to remove the most recent dialogue block added to the stack
		void Pop()
		{
			dialogueBlocks.TryPop(out _);
		}

		bool IsValidLine(string rawLine)
		{
			return !string.IsNullOrWhiteSpace(rawLine) && !rawLine.TrimStart().StartsWith(CommentLineDelimiter);
		}
	}
}
