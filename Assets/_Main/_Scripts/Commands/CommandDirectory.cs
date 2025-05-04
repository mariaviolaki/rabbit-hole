using System;
using System.Collections.Generic;
using UnityEngine;

namespace Commands
{
	public class CommandDirectory
	{
		// Command Manager will search our scripts and dynamically populate these with every class inheriting from DialogueCommand
		readonly Dictionary<string, Delegate> commandDirectory = new Dictionary<string, Delegate>();
		readonly Dictionary<string, Delegate> skipCommandDirectory = new Dictionary<string, Delegate>();

		public bool HasCommand(string commandName) => commandDirectory.ContainsKey(commandName);
		public bool HasSkipCommand(string commandName) => skipCommandDirectory.ContainsKey(commandName);

		public Delegate GetCommand(string commandName)
		{
			if (!HasCommand(commandName))
			{
				Debug.LogError($"{commandName} is not registered to the Command Directory!");
				return null;
			}

			return commandDirectory[commandName];
		}

		public Delegate GetSkipCommand(string commandName)
		{
			if (!HasSkipCommand(commandName))
			{
				Debug.LogError($"{commandName} is not registered to the Skip Command Directory!");
				return null;
			}

			return skipCommandDirectory[commandName];
		}

		public void AddCommand(string commandName, Delegate command, Delegate skipCommand = null)
		{
			if (HasCommand(commandName))
			{
				Debug.LogError($"Command Directory already includes {commandName}!");
				return;
			}

			commandDirectory[commandName] = command;
			skipCommandDirectory[commandName] = skipCommand;
		}
	}
}