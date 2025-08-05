namespace Dialogue
{
	public class DialogueNodeCommand
	{
		public string Name { get; private set; }
		public DialogueCommandArguments Arguments { get; private set; }
		public bool IsWaiting { get; private set; }

		public DialogueNodeCommand(string name, DialogueCommandArguments arguments, bool isWaiting)
		{
			Name = name;
			Arguments = arguments;
			IsWaiting = isWaiting;
		}
	}
}
