using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
	// The grouping of all dialogue commands available to be run
	CommandDirectory commandDirectory;
	// Used for certain commands which are not instantly executed - only one can be executed at a time
	Coroutine commandProcess;

	void Awake()
	{
		InitDirectory();
	}

	public Coroutine Execute(string commandName, params string[] arguments)
	{
		Delegate command = commandDirectory.GetCommand(commandName);
		if (command == null) return null;

		// Don't run commands simultaneously - we only keep track of one at a time
		ClearLastCommand();

		commandProcess = StartCoroutine(WaitForExecution(command, arguments));
		return commandProcess;
	}

	void ClearLastCommand()
	{
		if (commandProcess == null) return;

		StopCoroutine(commandProcess);
	}

	IEnumerator WaitForExecution(Delegate command, string[] arguments)
	{
		if (command is Action)
			command.DynamicInvoke();
		else if (command is Action<string>)
			command.DynamicInvoke(arguments[0]);
		else if (command is Action<string[]>)
			command.DynamicInvoke((object)arguments);
		else if (command is Func<IEnumerator>)
			yield return command.DynamicInvoke();
		else if (command is Func<string, IEnumerator>)
			yield return command.DynamicInvoke(arguments[0]);
		else if (command is Func<string[], IEnumerator>)
			yield return command.DynamicInvoke((object)arguments);

		commandProcess = null;
	}

	void InitDirectory()
	{
		commandDirectory = new CommandDirectory();

		Assembly projectAssembly = Assembly.GetExecutingAssembly();
		Type[] commandTypes = projectAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(DialogueCommands.DialogueCommand))).ToArray();
		
		foreach (Type commandType in commandTypes)
		{
			// Search for all scripts inheriting from DialogueCommand and register their commands to the directory 
			MethodInfo registerMethod = commandType.GetMethod("Register");
			if (registerMethod != null)
				registerMethod.Invoke(null, new object[] { commandDirectory });
			else
				Debug.LogError($"Register function not found in {commandType.Name}!");
		}
	}
}
