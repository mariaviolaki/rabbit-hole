using Dialogue;
using System.Collections;

namespace Logic
{
	public abstract class LogicSegmentBase
	{
		public static bool Matches(string rawLine) => false;

		protected string rawLine;
		protected DialogueSystem dialogueSystem;

		public virtual bool IsBlocking => false;

		public LogicSegmentBase(DialogueSystem dialogueSystem, string rawLine)
		{
			this.dialogueSystem = dialogueSystem;
			this.rawLine = rawLine;
		}	

		protected static bool StartsWithKeyword(string rawLine, string keyword)
		{
			return rawLine != null && rawLine.TrimStart().ToLower().StartsWith(keyword);
		}
	}

	public abstract class BlockingLogicSegmentBase : LogicSegmentBase
	{
		protected BlockingLogicSegmentBase(DialogueSystem dialogueSystem, string rawLine) : base(dialogueSystem, rawLine) { }
		public abstract IEnumerator Execute();
	}

	public abstract class NonBlockingLogicSegmentBase : LogicSegmentBase
	{
		protected NonBlockingLogicSegmentBase(DialogueSystem dialogueSystem, string rawLine) : base(dialogueSystem, rawLine) { }
		public abstract void Execute();
	}
}
