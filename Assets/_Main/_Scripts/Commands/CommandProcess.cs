using Dialogue;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Commands
{
	public class CommandProcess
	{
		readonly string name;
		readonly DialogueCommandArguments arguments;
		readonly Delegate command;
		readonly CommandManager commandManager;
		readonly bool isBlocking;
		readonly CommandSkipType skipType;

		Coroutine commandCoroutine;
		Coroutine skipCommandCoroutine;

		public Action OnFullyCompleted;

		public string Name => name;
		public bool IsCompleted => commandCoroutine == null;

		public CommandProcess(string name, DialogueCommandArguments arguments, Delegate command, CommandManager commandManager, CommandSkipType skipType, bool isBlocking)
		{
			this.name = name;
			this.arguments = arguments;
			this.command = command;
			this.commandManager = commandManager;
			this.isBlocking = isBlocking;
			this.skipType = skipType;
		}

		public void Start()
		{
			commandCoroutine = commandManager.StartCoroutine(ExecuteProcess());
		}

		public void Stop()
		{
			if (isBlocking || commandCoroutine == null) return;

			commandManager.StopCoroutine(commandCoroutine);
			commandCoroutine = null;

			skipCommandCoroutine = commandManager.StartCoroutine(ExecuteProcess());

			if (skipType == CommandSkipType.None)
				OnFullyCompleted?.Invoke();
			else if (skipType == CommandSkipType.Immediate)
				skipCommandCoroutine = commandManager.StartCoroutine(ExecuteImmediate());
			else
				skipCommandCoroutine = commandManager.StartCoroutine(ExecuteProcess());
		}

		IEnumerator ExecuteProcess()
		{
			if (command is Func<DialogueCommandArguments, IEnumerator>)
				yield return command.DynamicInvoke(arguments);
			else if (command is Func<DialogueCommandArguments, Task>)
				yield return WaitForTask((Task)command.DynamicInvoke(arguments));

			if (commandCoroutine != null) commandCoroutine = null;
			else skipCommandCoroutine = null;

			OnFullyCompleted?.Invoke();
		}

		IEnumerator ExecuteImmediate()
		{
			arguments.AddNamedArgument("immediate", "true");

			if (command is Func<DialogueCommandArguments, IEnumerator>)
				yield return command.DynamicInvoke(arguments);
			else if (command is Func<DialogueCommandArguments, Task>)
				yield return WaitForTask((Task)command.DynamicInvoke(arguments));

			skipCommandCoroutine = null;

			OnFullyCompleted?.Invoke();
		}

		IEnumerator WaitForTask(Task task)
		{
			while (!task.IsCompleted) yield return null;

			if (task.IsFaulted)
				Debug.LogWarning($"CommandProcess '{name}' error: {task.Exception}");
		}
	}
}
