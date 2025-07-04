using System.Collections.Generic;

namespace Dialogue
{
	public class DialogueStack
	{
		readonly Stack<DialogueBlock> dialogueBlocks = new();

		public Stack<DialogueBlock> Blocks => dialogueBlocks;
		public bool IsEmpty => dialogueBlocks.Count == 0;

		public void Clear() => dialogueBlocks.Clear();

		public void AddBlock(string filePath, List<string> lines, int fileStartIndex = -1, int fileEndIndex = -1, int progress = 0)
		{
			DialogueBlock dialogueBlock = new(filePath, lines, fileStartIndex, fileEndIndex, progress);
			dialogueBlocks.Push(dialogueBlock);
		}

		public string GetCurrentLine()
		{
			DialogueBlock dialogueBlock = GetBlock();
			string line = dialogueBlock?.GetLine();

			while (!IsEmpty && line == null)
			{
				Pop();
				dialogueBlock = GetBlock();
				line = dialogueBlock?.GetLine();
			}

			return line;
		}

		// Try to get the most recent dialogue block added to the stack
		public DialogueBlock GetBlock()
		{
			dialogueBlocks.TryPeek(out DialogueBlock dialogueBlock);
			return dialogueBlock;
		}

		public bool Proceed(DialogueBlock dialogueBlock)
		{
			if (dialogueBlock == null) return false;

			return dialogueBlock.IncrementProgress();
		}

		// Try to remove the most recent dialogue block added to the stack
		void Pop()
		{
			dialogueBlocks.TryPop(out _);
		}
	}
}
