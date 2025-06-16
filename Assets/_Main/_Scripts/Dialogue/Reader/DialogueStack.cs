using System.Collections.Generic;

namespace Dialogue
{
	public class DialogueStack
	{
		const string CommentLineDelimiter = "//";

		readonly Stack<DialogueBlock> dialogueBlocks = new();

		public bool IsEmpty => dialogueBlocks.Count == 0;

		public void Add(List<string> lines, int progress = 0)
		{
			DialogueBlock dialogueBlock = new(lines, progress);
			dialogueBlocks.Push(dialogueBlock);
		}

		public string Get()
		{
			DialogueBlock dialogueBlock = Peek();
			if (dialogueBlock == null) return null;

			string rawLine = dialogueBlock.GetLine();

			while (!IsEmpty && !IsValidLine(rawLine))
			{
				Proceed();
				dialogueBlock = Peek();
				rawLine = dialogueBlock?.GetLine();
			}

			return IsValidLine(rawLine) ? rawLine : null;
		}

		// Try to increment the progress on the most recent dialogue block block
		public bool Proceed()
		{
			while (!IsEmpty)
			{
				DialogueBlock dialogueBlock = Peek();
				if (dialogueBlock == null) return false;

				bool isIncremented = dialogueBlock.IncrementProgress();
				if (isIncremented) return true;

				// If the progress could not be incremented on this block, assume that it's complete
				Pop();
			}

			return false;
		}

		// Try to get the most recent dialogue block added to the stack
		DialogueBlock Peek()
		{
			dialogueBlocks.TryPeek(out DialogueBlock dialogueBlock);
			return dialogueBlock;
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
