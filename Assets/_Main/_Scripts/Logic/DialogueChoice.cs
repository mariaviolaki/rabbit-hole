using System.Collections.Generic;

namespace Logic
{
	public class DialogueChoice
	{
		readonly List<string> dialogueLines = new();
		readonly int index;
		readonly string text;
		readonly string filePath;
		readonly int fileStartIndex;
		int fileEndIndex;

		public List<string> DialogueLines => dialogueLines;
		public int Index => index;
		public string Text => text;
		public string FilePath => filePath;
		public int FileStartIndex => fileStartIndex;
		public int FileEndIndex { get { return fileEndIndex; } set { fileEndIndex = value; } }

		public DialogueChoice(int index, string text, string filePath, int fileStartIndex, int fileEndIndex = -1)
		{
			this.index = index;
			this.text = text;
			this.filePath = filePath;
			this.fileStartIndex = fileStartIndex;
			this.fileEndIndex = fileEndIndex;
		}
	}
}
