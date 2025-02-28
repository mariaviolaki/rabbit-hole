using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DialogueCommandData
{
	public class Command
	{
		public string Name { get; private set; }
		public string[] Arguments { get; private set; }

		public Command(string name, string[] arguments)
		{
			Name = name;
			Arguments = arguments;
		}
	}

	const char CommandDelimiter = ',';
	const char CommandNameDelimiter = '(';
	const char CommandEndDelimiter = ')';

	public List<Command> CommandList { get; private set; }

	public DialogueCommandData(string rawCommands)
	{
		ParseCommandData(rawCommands);
	}

	public void ParseCommandData(string argumentsString)
	{
		CommandList = new List<Command>();
		List<string> arguments = new List<string>();
		StringBuilder currentCommandName = new StringBuilder();
		StringBuilder currentArgument = new StringBuilder();

		bool isInQuotes = false;
		bool isValidArgString = false;
		bool hasOpenArgsDelimiter = false;
		bool hasClosedArgsDelimiter = false;

		foreach (char character in argumentsString)
		{
			if (char.IsWhiteSpace(character) && !isInQuotes) continue;

			if (!hasOpenArgsDelimiter)
				hasOpenArgsDelimiter = character == CommandNameDelimiter && !isInQuotes;
			if (!hasClosedArgsDelimiter)
				hasClosedArgsDelimiter = character == CommandEndDelimiter && !isInQuotes;
			
			if (!hasOpenArgsDelimiter && !hasClosedArgsDelimiter)
			{
				currentCommandName.Append(character);
			}
			else if (hasOpenArgsDelimiter && hasClosedArgsDelimiter)
			{
				if (character == CommandDelimiter)
				{
					hasOpenArgsDelimiter = false;
					hasClosedArgsDelimiter = false;
				}
				else if (character == CommandEndDelimiter)
				{
					// Add the currently processed command to the list of commands
					UpdateCommands(currentCommandName, currentArgument, arguments, isValidArgString);
				}
			}
			else if (hasOpenArgsDelimiter && !hasClosedArgsDelimiter)
			{
				if (character == CommandNameDelimiter && !isInQuotes) continue;

				if (character == '"')
				{
					isInQuotes = !isInQuotes;
					isValidArgString = !isInQuotes;
				}
				else if (character == CommandDelimiter && !isInQuotes)
				{
					// Add the currently processed argument to the list of arguments
					UpdateLastArgument(currentArgument, arguments, isValidArgString);
					isValidArgString = false;
				}
				else
				{
					currentArgument.Append(character);
					isValidArgString = false;
				}
			}
		}
	}

	// Runs after the entire command is processed
	void UpdateCommands(StringBuilder nameBuilder, StringBuilder lastArgumentBuilder, List<string> arguments, bool isValidArgString)
	{
		// Use the string in the name builder as command name and clear it for the next command
		string commandName = nameBuilder.ToString().Trim();
		nameBuilder.Clear();

		// If the last argument in the builder is valid, add it as a param to the command
		string trimmedArgument = lastArgumentBuilder.ToString().Trim();
		if (trimmedArgument.Length > 0 || isValidArgString)
		{
			arguments.Add(lastArgumentBuilder.ToString().Trim());
			lastArgumentBuilder.Clear();
		}

		// Save the name and all the argumens of this command
		CommandList.Add(new Command(commandName, arguments.ToArray()));
		arguments.Clear();
	}

	// Runs after each command argument is processed
	void UpdateLastArgument(StringBuilder currentArgumentBuilder, List<string> arguments, bool isValidArgString)
	{
		// Add the string in the builder as an argument for the current command being processed
		string trimmedArgument = currentArgumentBuilder.ToString().Trim();
		if (trimmedArgument.Length > 0 || isValidArgString)
			arguments.Add(trimmedArgument);

		currentArgumentBuilder.Clear();
	}
}
