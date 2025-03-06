namespace Dialogue
{
	public class DialogueLine
	{
		public DialogueSpeakerData Speaker { get; private set; }
		public DialogueTextData Dialogue { get; private set; }
		public DialogueCommandData Commands { get; private set; }

		public DialogueLine(string speaker, string dialogue, string commands)
		{
			Speaker = string.IsNullOrWhiteSpace(speaker) ? null : new DialogueSpeakerData(speaker);
			Dialogue = string.IsNullOrWhiteSpace(dialogue) ? null : new DialogueTextData(dialogue);
			Commands = string.IsNullOrWhiteSpace(commands) ? null : new DialogueCommandData(commands);
		}
	}
}