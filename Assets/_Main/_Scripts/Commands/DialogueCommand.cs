namespace Commands
{
	public abstract class DialogueCommand
	{
		// Classes inheriting from DialogueCommand must redefine this function
		public static void Register(CommandManager commandManager) { }
	}
}
