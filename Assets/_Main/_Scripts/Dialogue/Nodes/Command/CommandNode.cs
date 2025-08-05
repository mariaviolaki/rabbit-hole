using Commands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dialogue
{
	public class CommandNode : NodeBase
	{
		// This is also used by the DialogueParser to split commands from dialogue text
		readonly static string CommandWaitKeyword = "wait";
		public readonly static string CommandPattern = $@"({CommandWaitKeyword}\s+)?[a-zA-Z][a-zA-Z0-9]*\s*(\.\s*[a-zA-Z][a-zA-Z0-9]*\s*)?\(";

		readonly string CommandDelimiterPattern = $@"\)\s*,\s*{CommandPattern}";
		const char CommandDelimiter = ',';
		const char ArgsDelimiter = ',';
		const char OpenArgsDelimiter = '(';
		const char ClosedArgsDelimiter = ')';

		readonly List<DialogueNodeCommand> commands = new();
		readonly CommandManager commandManager;
		bool isWaitingToAdvance;

		public CommandNode(DialogueTreeNode treeNode, DialogueFlowController flowController) : base(treeNode, flowController)
		{
			commandManager = flowController.Dialogue.Commands;
		}

		public override void StartExecution()
		{
			if (IsExecuting) return;
			base.StartExecution();
			ParseTreeNode();
			executionCoroutine = flowController.Dialogue.StartCoroutine(ExecuteLogic());
		}

		protected override void ParseTreeNode()
		{
			ParseCommandData(treeNode.Data[0].Trim());
		}

		protected override IEnumerator ExecuteLogic()
		{
			List<CommandProcess> processesToWait = new();

			foreach (DialogueNodeCommand command in commands)
			{
				CommandProcess process = commandManager.Execute(command.Name, command.Arguments);
				if (process == null) continue;

				if (process.IsBlocking || command.IsWaiting || command.Name.ToLower() == "wait")
					processesToWait.Add(process);
			}

			// Wait to execute all processes of this line concurrently
			if (processesToWait.Count > 0)
			{
				isWaitingToAdvance = true;
				while (true)
				{
					// Stop when all processes end, or the user clicks to skip them
					if (processesToWait.All(p => p.IsCompleted)) break;
					else if (!flowController.IsRunning || !isWaitingToAdvance || flowController.IsSkipping)
					{
						commandManager.SkipCommands();
						break;
					}
					yield return null;
				}

				// Wait for any previous skipped transitions to complete smoothly
				while (!commandManager.IsIdle()) yield return null;
			}

			executionCoroutine = null;
			flowController.ProceedToNode(treeNode.NextId);
		}

		public override void SpeedUpExecution()
		{
			base.SpeedUpExecution();
			isWaitingToAdvance = false;
		}

		void ParseCommandData(string rawCommands)
		{
			Regex commandDelimiterRegex = new(CommandDelimiterPattern);
			MatchCollection delimiterMatches = commandDelimiterRegex.Matches(rawCommands);

			if (delimiterMatches.Count == 0)
			{
				ParseCommandFromString(rawCommands.Trim());
				return;
			}

			string firstCommandString = rawCommands.Substring(0, delimiterMatches[0].Index + delimiterMatches[0].Value.IndexOf(CommandDelimiter));
			ParseCommandFromString(firstCommandString.Trim());

			for (int i = 0; i < delimiterMatches.Count; i++)
			{
				Match match = delimiterMatches[i];
				Match nextMatch = i + 1 == delimiterMatches.Count ? null : delimiterMatches[i + 1];

				int commandStart = match.Index + match.Value.IndexOf(CommandDelimiter) + 1;
				int commandEnd = nextMatch == null ? rawCommands.Length : nextMatch.Index + nextMatch.Value.IndexOf(CommandDelimiter);

				string commandString = rawCommands.Substring(commandStart, commandEnd - commandStart);
				ParseCommandFromString(commandString.Trim());
			}
		}

		void ParseCommandFromString(string commandString)
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

			DialogueCommandArguments commandArguments = new DialogueCommandArguments(arguments);
			DialogueNodeCommand command = new(name, commandArguments, isWaiting);
			commands.Add(command);
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
