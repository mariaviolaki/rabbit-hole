using Logic;

namespace Dialogue
{
	public class DialogueLine
	{
		public string FilePath { get; private set; }
		public int LineNumber { get; private set; }
		public LogicSegmentBase Logic { get; private set; } = null;
		public DialogueSpeakerData Speaker { get; private set; } = null;
		public DialogueTextData Dialogue { get; private set; } = null;
		public DialogueCommandData Commands { get; private set; } = null;
		
		public DialogueLine(string filePath, int lineNumber, LogicSegmentBase logicSegment)
		{
			FilePath = filePath;
			LineNumber = lineNumber;
			Logic = logicSegment;
		}

		public DialogueLine(string filePath, int lineNumber, string speaker, string dialogue, string commands)
		{
			FilePath = filePath;
			LineNumber = lineNumber;
			Speaker = string.IsNullOrWhiteSpace(speaker) ? null : new DialogueSpeakerData(speaker);
			Dialogue = string.IsNullOrWhiteSpace(dialogue) ? null : new DialogueTextData(dialogue);
			Commands = string.IsNullOrWhiteSpace(commands) ? null : new DialogueCommandData(commands);
		}
	}
}