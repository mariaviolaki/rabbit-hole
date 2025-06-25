using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
	public class DialogueBlock
	{
		readonly List<string> rawLines = new();
		readonly string filePath;
		int fileStartIndex;
		int fileEndIndex;
		int progress;

		public List<string> Lines => rawLines;
		public string FilePath => filePath;
		public int FileStartIndex => fileStartIndex;
		public int FileEndIndex => fileEndIndex;
		public int Progress => progress;
		public bool IsComplete => progress == rawLines.Count;

		public DialogueBlock(string filePath, List<string> rawLines, int fileStartIndex = -1, int fileEndIndex = -1, int progress = 0)
		{
			this.rawLines = new List<string>(rawLines);
			this.filePath = filePath;
			this.fileStartIndex = (fileStartIndex == -1) ? 0 : fileStartIndex;
			this.fileEndIndex = (fileEndIndex == -1) ? rawLines.Count - 1 : fileEndIndex;
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

		public void SetProgress(int progress)
		{
			this.progress = Mathf.Clamp(progress, 0, rawLines.Count);
		}

		public void LoadProgress(int progress, int fileStartIndex, int fileEndIndex)
		{
			SetProgress(progress);
			this.fileStartIndex = Mathf.Clamp(fileStartIndex, 0, rawLines.Count);
			this.fileEndIndex = Mathf.Clamp(fileEndIndex, 0, rawLines.Count);
		}
	}
}
