using System.Collections.Generic;

namespace Logic
{
	public class LogicBlock
	{
		readonly List<string> lines;
		readonly int startIndex;
		readonly int endIndex;
		readonly string filePath;
		readonly int fileStartIndex;
		readonly int fileEndIndex;

		public List<string> Lines => lines;
		public int StartIndex => startIndex;
		public int EndIndex => endIndex;
		public string FilePath => filePath;
		public int FileStartIndex => fileStartIndex;
		public int FileEndIndex => fileEndIndex;

		public LogicBlock(int startIndex, int endIndex, List<string> lines, string filePath, int fileStartIndex, int fileEndIndex)
		{
			this.startIndex = startIndex;
			this.endIndex = endIndex;
			this.lines = lines;
			this.filePath = filePath;
			this.fileStartIndex = fileStartIndex;
			this.fileEndIndex = fileEndIndex;
		}
	}
}
