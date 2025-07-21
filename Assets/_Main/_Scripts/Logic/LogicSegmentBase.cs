using Dialogue;
using System.Collections;

namespace Logic
{
	public abstract class LogicSegmentBase
	{
		public static bool Matches(string rawLine) => false;

		protected string rawLine;
		protected DialogueManager dialogueManager;

		public virtual bool IsBlocking => false;

		public LogicSegmentBase(DialogueManager dialogueManager, string rawLine)
		{
			this.dialogueManager = dialogueManager;
			this.rawLine = rawLine;
		}	

		protected static bool StartsWithKeyword(string rawLine, string keyword)
		{
			return rawLine != null && rawLine.TrimStart().ToLower().StartsWith(keyword);
		}
	}

	public abstract class BlockingLogicSegmentBase : LogicSegmentBase
	{
		protected BlockingLogicSegmentBase(DialogueManager dialogueManager, string rawLine) : base(dialogueManager, rawLine) { }
		public abstract IEnumerator Execute();
		public abstract IEnumerator ForceComplete();
	}

	public abstract class NonBlockingLogicSegmentBase : LogicSegmentBase
	{
		protected NonBlockingLogicSegmentBase(DialogueManager dialogueManager, string rawLine) : base(dialogueManager, rawLine) { }
		public abstract void Execute();
	}
}
