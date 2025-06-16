using System.Collections.Generic;

namespace Dialogue
{
	public class DialogueBlock
	{
		readonly List<string> rawLines = new();
		int progress = 0;

		public List<string> Lines => rawLines;
		public int Progress => progress;
		public bool IsComplete => progress == rawLines.Count;

		public DialogueBlock(List<string> rawLines, int progress = 0)
		{
			this.rawLines = new List<string>(rawLines);
			this.progress = progress;
		}

		public string GetLine()
		{
			if (IsComplete) return null;

			return Lines[progress];
		}

		public bool IncrementProgress()
		{
			if (IsComplete) return false;

			progress++;
			return true;
		}
	}
}
