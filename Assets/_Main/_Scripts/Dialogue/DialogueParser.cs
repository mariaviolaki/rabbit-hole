using System.Text.RegularExpressions;

public static class DialogueParser
{
	const string CommandRegexPattern = @"\w+\S\(";

	public static DialogueLine Parse(string rawLine)
	{
		string speaker = "", dialogue = "", commands = "";

		Regex commandRegex = new Regex(CommandRegexPattern);
		MatchCollection commandMatches = commandRegex.Matches(rawLine);
		int firstCommandStart = commandMatches.Count == 0 ? -1 : commandMatches[0].Index;

		(int dialogueStart, int dialogueEnd) = GetDialogueBounds(rawLine, firstCommandStart);
		int commandsStart = GetCommandsStart(commandMatches, dialogueEnd);

		int speakerLength = dialogueStart - 1;
		int dialogueLength = dialogueEnd - dialogueStart - 1;
		int commandsLength = rawLine.Length - commandsStart;

		if (dialogueStart > -1 && speakerLength > 0 && dialogueLength > 0)
		{
			speaker = rawLine.Substring(0, speakerLength)?.Trim();
			dialogue = rawLine.Substring(dialogueStart + 1, dialogueLength)?.Replace("\\\"", "\"").Trim();
		}

		if (commandsLength > 0 && commandsStart > -1)
			commands = rawLine.Substring(commandsStart, commandsLength)?.Trim();

		return new DialogueLine(speaker, dialogue, commands);
	}

	static (int, int) GetDialogueBounds(string rawLine, int firstCommandStart)
	{
		int dialogueStart = -1;
		int dialogueEnd = -1;

		bool isEscaped = false;
		for (int i = 0; i < rawLine.Length; i++)
		{
			char current = rawLine[i];

			if (isEscaped)
			{
				isEscaped = false;
			}
			else
			{
				if (current == '\\')
				{
					isEscaped = true;
				}
				else if (current == '\"')
				{
					if (dialogueStart == -1)
					{
						dialogueStart = i;

						// Any dialogue commands should always follow the dialogue text
						if (firstCommandStart != -1 && firstCommandStart < dialogueStart)
							return (-1, -1);
					}
					else if (dialogueEnd == -1)
					{
						dialogueEnd = i;

						// A valid start and end of the dialogue has been found
						return (dialogueStart, dialogueEnd);
					}
				}
			}
		}

		return (-1, -1);
	}

	static int GetCommandsStart(MatchCollection commandMatches, int dialogueEnd)
	{
		foreach (Match commandMatch in commandMatches)
		{
			if (commandMatch.Index > dialogueEnd)
				return commandMatch.Index;
		}

		return -1;
	}
}
