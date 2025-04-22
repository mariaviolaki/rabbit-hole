using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Dialogue
{
	public class DialogueCommandData
	{
		public class Command
		{
			public string Name { get; private set; }
			public string[] Arguments { get; private set; }
			public bool IsWaiting { get; private set; }

			public Command(string name, string[] arguments, bool isWaiting)
			{
				Name = name;
				Arguments = arguments;
				IsWaiting = isWaiting;
			}
		}

		const string CommandWaitKeyword = "wait";
		readonly string CommandDelimiterPattern = $@"\)\s*,\s*({CommandWaitKeyword}\s+)?[a-zA-Z][a-zA-Z0-9]*\s*\(";
		const char CommandDelimiter = ',';
		const char ArgsDelimiter = ',';
		const char OpenArgsDelimiter = '(';
		const char ClosedArgsDelimiter = ')';

		public List<Command> CommandList { get; private set; }

		public DialogueCommandData(string rawCommands)
		{
			ParseCommandData(rawCommands);
		}

		void ParseCommandData(string rawCommands)
		{
			CommandList = new List<Command>();

			Regex commandDelimiterRegex = new Regex(CommandDelimiterPattern);
			MatchCollection delimiterMatches = commandDelimiterRegex.Matches(rawCommands);

			List<string> commandStrings = new List<string>();

			if (delimiterMatches.Count == 0)
			{
				AddCommandFromString(rawCommands.Trim());
				return;
			}

			string firstCommandString = rawCommands.Substring(0, delimiterMatches[0].Index + delimiterMatches[0].Value.IndexOf(CommandDelimiter));
			AddCommandFromString(firstCommandString.Trim());

			for (int i = 0; i < delimiterMatches.Count; i++)
			{
				Match match = delimiterMatches[i];
				Match nextMatch = i + 1 == delimiterMatches.Count ? null : delimiterMatches[i + 1];

				int commandStart = match.Index + match.Value.IndexOf(CommandDelimiter) + 1;
				int commandEnd = nextMatch == null ? rawCommands.Length : nextMatch.Index + nextMatch.Value.IndexOf(CommandDelimiter);

				string commandString = rawCommands.Substring(commandStart, commandEnd - commandStart);
				AddCommandFromString(commandString.Trim());
			}
		}

		void AddCommandFromString(string commandString)
		{
			bool isWaiting = commandString.StartsWith(CommandWaitKeyword + " ");
			string name = null;
			List<string> arguments = new List<string>();

			int startIndex = isWaiting ? (CommandWaitKeyword + " ").Length : 0;
			bool isEscaped = false;
			bool isInQuotes = false;
			bool isFullStringInQuotes = false;

			StringBuilder nameBuilder = new StringBuilder();
			StringBuilder argumentBuilder = new StringBuilder();

			for (int i = startIndex; i < commandString.Length; i++)
			{
				char character = commandString[i];
				if (isFullStringInQuotes && char.IsWhiteSpace(character)) continue;

				if (character == '\\' && i + 1 != commandString.Length && commandString[i + 1] == '"')
				{
					isEscaped = true;
					continue;
				}
				else if (character == '"' && !isEscaped)
				{
					isInQuotes = !isInQuotes;
					isFullStringInQuotes = !isInQuotes;

					// Clear the argument builder before adding what is inside quotes
					// Text inside quotes is saved untrimmed
					if (isInQuotes)
						argumentBuilder.Clear();

					continue;
				}

				if (character == OpenArgsDelimiter && !isInQuotes && name == null)
				{
					name = GetStringFromBuilder(nameBuilder, false);
				}
				else if (character == ArgsDelimiter && !isInQuotes)
				{
					string argument = GetStringFromBuilder(argumentBuilder, isFullStringInQuotes);
					if (argument != null)
						arguments.Add(argument);
				}
				else if (character == ClosedArgsDelimiter && !isInQuotes)
				{
					string argument = GetStringFromBuilder(argumentBuilder, isFullStringInQuotes);
					if (argument != null)
						arguments.Add(argument);
					break;
				}
				else if (name == null)
				{
					nameBuilder.Append(character);
				}
				else
				{
					argumentBuilder.Append(character);
				}

				isEscaped = false;
				isFullStringInQuotes = false;
			}

			if (string.IsNullOrWhiteSpace(name)) return;

			Command command = new Command(name, arguments.ToArray(), isWaiting);
			CommandList.Add(command);
		}

		string GetStringFromBuilder(StringBuilder stringBuilder, bool isFullStringInQuotes)
		{
			// Don't return empty strings unless they are in quotes
			string stringText = isFullStringInQuotes ? stringBuilder.ToString() : stringBuilder.ToString().Trim();
			stringBuilder.Clear();

			return stringText.Length > 0 || isFullStringInQuotes ? stringText : null;
		}
	}
}