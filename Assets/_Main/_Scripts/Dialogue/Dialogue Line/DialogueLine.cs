public class DialogueLine
{
	public string Speaker { get; private set; }
	public DialogueTextData Dialogue { get; private set; }
	public string Commands { get; private set; }

	public DialogueLine(string speaker, string dialogue, string commands)
	{
		Speaker = string.IsNullOrWhiteSpace(speaker) ? null : speaker;
		Dialogue =  string.IsNullOrWhiteSpace(dialogue) ? null : new DialogueTextData(dialogue);
		Commands = string.IsNullOrWhiteSpace(commands) ? null : commands;
	}
}
