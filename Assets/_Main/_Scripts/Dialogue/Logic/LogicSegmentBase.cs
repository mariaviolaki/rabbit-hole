using System.Collections;

namespace Dialogue
{
	public abstract class LogicSegmentBase
	{
		public static string Keyword => null;

		protected string rawData;
		protected DialogueSystem dialogueSystem;

		public abstract bool IsComplete { get; }

		public LogicSegmentBase(DialogueSystem dialogueSystem, string rawData)
		{
			this.dialogueSystem = dialogueSystem;
			this.rawData = rawData;
		}

		public abstract IEnumerator Execute();
	}
}
