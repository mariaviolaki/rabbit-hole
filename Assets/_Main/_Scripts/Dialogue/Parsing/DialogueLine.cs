using Logic;

namespace Dialogue
{
	public class DialogueLine
	{
		public LogicSegmentBase Logic { get; private set; } = null;
		public DialogueSpeakerData Speaker { get; private set; } = null;
		public DialogueTextData Dialogue { get; private set; } = null;
		public DialogueCommandData Commands { get; private set; } = null;
		
		public DialogueLine(LogicSegmentBase logicSegment)
		{
			Logic = logicSegment;
		}

		public DialogueLine(string speaker, string dialogue, string commands)
		{
			Speaker = string.IsNullOrWhiteSpace(speaker) ? null : new DialogueSpeakerData(speaker);
			Dialogue = string.IsNullOrWhiteSpace(dialogue) ? null : new DialogueTextData(dialogue);
			Commands = string.IsNullOrWhiteSpace(commands) ? null : new DialogueCommandData(commands);
		}
	}
}