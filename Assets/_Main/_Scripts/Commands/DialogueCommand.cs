
namespace DialogueCommands
{
	public abstract class DialogueCommand
	{
		// Classes inheriting from DialogueCommand must redefine this function
		public static void Register(CommandDirectory commandDirectory) { }
	}
}
