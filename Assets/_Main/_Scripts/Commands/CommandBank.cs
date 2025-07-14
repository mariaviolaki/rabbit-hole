using System;
using System.Collections.Generic;
using UnityEngine;

namespace Commands
{
	public class CommandBank
	{
		// Command Manager will search our scripts and dynamically populate these with every class inheriting from DialogueCommand
		readonly Dictionary<string, Delegate> commandBank = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, CommandSkipType> commandSkipTypes = new(StringComparer.OrdinalIgnoreCase);

		public bool HasCommand(string commandName) => commandBank.ContainsKey(commandName);
		public bool HasSkipCommand(string commandName) => commandSkipTypes.ContainsKey(commandName);

		public Delegate GetCommand(string commandName)
		{
			if (!HasCommand(commandName))
			{
				Debug.LogError($"{commandName} is not registered to the Command Bank!");
				return null;
			}

			return commandBank[commandName];
		}

		public CommandSkipType GetSkipType(string commandName)
		{
			if (!HasSkipCommand(commandName))
			{
				Debug.LogError($"{commandName} is not registered to the Skip Command Bank!");
				return CommandSkipType.Default;
			}

			return commandSkipTypes[commandName];
		}

		public void AddCommand(string commandName, Delegate command, CommandSkipType skipType = CommandSkipType.Default)
		{
			if (HasCommand(commandName))
			{
				Debug.LogError($"Command Bank already includes {commandName}!");
				return;
			}

			commandBank[commandName] = command;
			commandSkipTypes[commandName] = skipType;
		}
	}
}