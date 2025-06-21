using System.Collections.Generic;

namespace Logic
{
	public class LogicBlock
	{
		readonly int startIndex = -1;
		readonly int endIndex = -1;
		readonly List<string> lines;

		public int StartIndex => startIndex;
		public int EndIndex => endIndex;
		public List<string> Lines => lines;

		public LogicBlock(int startIndex, int endIndex, List<string> lines)
		{
			this.startIndex = startIndex;
			this.endIndex = endIndex;
			this.lines = lines;
		}
	}
}
