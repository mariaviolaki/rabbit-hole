using Logic;
using System.Collections;

namespace Dialogue
{
	public class DialogueLogicReader
	{
		readonly LogicSegmentManager logicSegmentManager;

		BlockingLogicSegmentType currentLogicSegment;

		public LogicSegmentManager LogicManager => logicSegmentManager;
		public BlockingLogicSegmentType LogicSegmentType { get { return currentLogicSegment; } set { currentLogicSegment = value; } }

		public DialogueLogicReader(LogicSegmentManager logicSegmentManager)
		{
			this.logicSegmentManager = logicSegmentManager;
		}

		public IEnumerator ProcessLogicSegment(DialogueLine line)
		{
			// Reset the previous line's logic segment right before it gets updated with the new one
			currentLogicSegment = BlockingLogicSegmentType.None;

			// If there no logic to be processed in this line, stop early
			if (line.Logic == null) yield break;

			logicSegmentManager.Add(line.Logic);

			// When execution is finished, the stack will have been updated
			if (logicSegmentManager.HasPendingLogic)
				yield return logicSegmentManager.WaitForExecution();
		}
	}
}
