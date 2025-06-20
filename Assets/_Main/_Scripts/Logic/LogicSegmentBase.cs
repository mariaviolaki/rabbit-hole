using Dialogue;
using System.Collections;

namespace Logic
{
	public abstract class LogicSegmentBase
	{
		public static bool Matches(string rawLine) => false;

		protected string rawLine;
		protected DialogueSystem dialogueSystem;

		public LogicSegmentBase(DialogueSystem dialogueSystem, string rawLine)
		{
			this.dialogueSystem = dialogueSystem;
			this.rawLine = rawLine;
		}

		public abstract IEnumerator Execute();

		protected static bool StartsWithKeyword(string rawLine, string keyword)
		{
			return rawLine != null && rawLine.TrimStart().ToLower().StartsWith(keyword);
		}
	}
}
