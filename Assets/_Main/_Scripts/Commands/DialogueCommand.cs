namespace Commands
{
	public abstract class DialogueCommand
	{
		public static readonly string[] blockingProcesses = new string[] { };

		// Classes inheriting from DialogueCommand must redefine this function
		public static void Register(CommandManager commandManager) { }
	}
}
