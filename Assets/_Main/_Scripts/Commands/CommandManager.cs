using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Characters;
using UnityEngine;

namespace Commands
{
	public class CommandManager : MonoBehaviour
	{
		// The grouping of all dialogue commands available to be run
		CommandDirectory commandDirectory;
		CharacterManager characterManager;

		// Keep track of all the processes that may run simultaneously
		readonly Dictionary<Guid, CommandProcess> processes = new Dictionary<Guid, CommandProcess>();
		readonly HashSet<string> unskippableProcessNames = new HashSet<string>();

		public bool IsIdle => processes.Count == 0;

		void Awake()
		{
			InitDirectory();
		}

		public CommandProcess Execute(string commandName, params string[] arguments)
		{
			Delegate command = commandDirectory.GetCommand(commandName);
			Delegate skipCommand = commandDirectory.GetSkipCommand(commandName);
			if (command == null) return null;

			if (command.Method.ReturnType == typeof(void))
			{
				ExecuteAction(command, arguments);
				return null;
			}
			else if (command.Method.ReturnType == typeof(IEnumerator))
			{
				CommandProcess process = ExecuteProcess(command, skipCommand, commandName, arguments);
				return process;
			}

			return null;
		}

		public void SkipCommands()
		{
			// Iterate over a copy of the list to avoid conflicts in case the processes change in the meantime
			foreach (CommandProcess process in processes.Values.ToList())
			{
				process.Stop();
			}
		}

		void DeleteProcess(Guid processId)
		{
			processes.Remove(processId);
		}

		void ExecuteAction(Delegate command, string[] arguments)
		{
			if (command is Action)
				command.DynamicInvoke();
			else if (command is Action<string>)
				command.DynamicInvoke(arguments[0]);
			else if (command is Action<string[]>)
				command.DynamicInvoke((object)arguments);
		}

		CommandProcess ExecuteProcess(Delegate command, Delegate skipCommand, string commandName, string[] arguments)
		{
			Guid processId = Guid.NewGuid();
			bool isUnskippable = unskippableProcessNames.Contains(commandName);

			CommandProcess commandProcess = new CommandProcess(commandName, arguments, command, skipCommand, this, isUnskippable);
			commandProcess.OnFullyCompleted += () => DeleteProcess(processId);
			commandProcess.Start();
			processes.Add(processId, commandProcess);

			return commandProcess;
		}

		void InitDirectory()
		{
			characterManager = FindObjectOfType<CharacterManager>();
			commandDirectory = new CommandDirectory(characterManager);

			Assembly projectAssembly = Assembly.GetExecutingAssembly();
			Type[] commandTypes = projectAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(DialogueCommand))).ToArray();

			foreach (Type commandType in commandTypes)
			{
				// Search for all scripts inheriting from DialogueCommand and register their commands to the directory 
				MethodInfo registerMethod = commandType.GetMethod("Register", BindingFlags.Static | BindingFlags.Public);
				if (registerMethod != null)
					registerMethod.Invoke(null, new object[] { commandDirectory });
				else
					Debug.LogError($"Register function not found in {commandType.Name}!");

				// If any unskippable processes are included, cache them in a hash set
				FieldInfo unskippableProcessesField = commandType.GetField("unskippableProcesses", BindingFlags.Static | BindingFlags.Public);
				if (unskippableProcessesField != null)
				{
					string[] unskippableProcessNames = (string[])unskippableProcessesField.GetValue(null);
					foreach (string processName in unskippableProcessNames)
					{
						this.unskippableProcessNames.Add(processName);
					}
				}
			}
		}
	}
}
