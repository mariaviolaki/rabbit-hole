using System;
using System.Collections.Generic;
using UnityEngine;

public class CommandDirectory
{
	// Command Manager will search our scripts and dynamically populate this with every class inheriting from DialogueCommand
	Dictionary<string, Delegate> directory = new Dictionary<string, Delegate>();

	public bool HasCommand(string commandName) => directory.ContainsKey(commandName);

	public Delegate GetCommand(string commandName)
	{
		if (!HasCommand(commandName))
		{
			Debug.LogError($"{commandName} is not registered to the Command Directory!");
			return null;
		}

		return directory[commandName];
	}

	public void AddCommand(string commandName, Delegate command)
	{
		if (HasCommand(commandName))
		{
			Debug.LogError($"Command Directory already includes {commandName}!");
			return;
		}

		directory[commandName] = command;
	}
}
