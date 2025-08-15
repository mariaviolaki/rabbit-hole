using Dialogue;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Commands
{
	public class CommandBank
	{
		// Command Manager will search our scripts and dynamically populate these with every class inheriting from DialogueCommand
		readonly Dictionary<string, Func<DialogueCommandArguments, CommandProcessBase>> commandBank = new(StringComparer.OrdinalIgnoreCase);

		public bool HasCommand(string commandName) => commandBank.ContainsKey(commandName);

		public Func<DialogueCommandArguments, CommandProcessBase> GetCommand(string commandName)
		{
			if (!commandBank.TryGetValue(commandName, out var commandProcess))
			{
				Debug.LogError($"{commandName} is not registered to the Command Bank!");
				return null;
			}

			return commandProcess;
		}

		public void AddCommand(string commandName, Func<DialogueCommandArguments, CommandProcessBase> command)
		{
			if (HasCommand(commandName))
			{
				Debug.LogError($"Command Bank already includes {commandName}!");
				return;
			}

			commandBank[commandName] = command;
		}
	}
}