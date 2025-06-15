using System;
using System.Collections.Generic;
using UnityEngine;

namespace Commands
{
	public class CommandDirectory
	{
		// Command Manager will search our scripts and dynamically populate these with every class inheriting from DialogueCommand
		readonly Dictionary<string, Delegate> commandDirectory = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<string, CommandSkipType> commandSkipTypes = new(StringComparer.OrdinalIgnoreCase);

		public bool HasCommand(string commandName) => commandDirectory.ContainsKey(commandName);
		public bool HasSkipCommand(string commandName) => commandSkipTypes.ContainsKey(commandName);

		public Delegate GetCommand(string commandName)
		{
			if (!HasCommand(commandName))
			{
				Debug.LogError($"{commandName} is not registered to the Command Directory!");
				return null;
			}

			return commandDirectory[commandName];
		}

		public CommandSkipType GetSkipType(string commandName)
		{
			if (!HasSkipCommand(commandName))
			{
				Debug.LogError($"{commandName} is not registered to the Skip Command Directory!");
				return CommandSkipType.None;
			}

			return commandSkipTypes[commandName];
		}

		public void AddCommand(string commandName, Delegate command, CommandSkipType skipType = CommandSkipType.None)
		{
			if (HasCommand(commandName))
			{
				Debug.LogError($"Command Directory already includes {commandName}!");
				return;
			}

			commandDirectory[commandName] = command;
			commandSkipTypes[commandName] = skipType;
		}
	}
}