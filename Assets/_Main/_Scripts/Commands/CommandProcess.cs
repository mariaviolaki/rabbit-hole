using System;
using System.Collections;
using UnityEngine;

namespace Commands
{
	public class CommandProcess
	{
		readonly string name;
		readonly string[] arguments;
		readonly Delegate command;
		readonly Delegate skipCommand;
		readonly CommandManager commandManager;
		readonly bool isUnskippable;

		Coroutine commandCoroutine;
		Coroutine skipCommandCoroutine;

		public Action OnFullyCompleted;

		public string Name => name;
		public bool IsCompleted => commandCoroutine == null;

		public CommandProcess(string name, string[] arguments, Delegate command, Delegate skipCommand, CommandManager commandManager, bool isUnskippable)
		{
			this.name = name;
			this.arguments = arguments;
			this.command = command;
			this.skipCommand = skipCommand;
			this.commandManager = commandManager;
			this.isUnskippable = isUnskippable;
		}

		public void Start()
		{
			commandCoroutine = commandManager.StartCoroutine(ExecuteProcess(command));
		}

		public void Stop()
		{
			if (isUnskippable || commandCoroutine == null) return;

			commandManager.StopCoroutine(commandCoroutine);
			commandCoroutine = null;

			if (skipCommand == null)
				OnFullyCompleted?.Invoke();
			else if (skipCommand.Method.ReturnType == typeof(void))
				ExecuteAction();
			else
				skipCommandCoroutine = commandManager.StartCoroutine(ExecuteProcess(skipCommand));
		}

		IEnumerator ExecuteProcess(Delegate command)
		{
			if (command is Func<IEnumerator>)
				yield return command.DynamicInvoke();
			else if (command is Func<string, IEnumerator>)
				yield return command.DynamicInvoke(arguments[0]);
			else if (command is Func<string[], IEnumerator>)
				yield return command.DynamicInvoke((object)arguments);

			if (commandCoroutine != null) commandCoroutine = null;
			else skipCommandCoroutine = null;

			OnFullyCompleted?.Invoke();
		}

		void ExecuteAction()
		{
			if (skipCommand is Action)
				skipCommand.DynamicInvoke();
			else if (skipCommand is Action<string>)
				skipCommand.DynamicInvoke(arguments[0]);
			else if (skipCommand is Action<string[]>)
				skipCommand.DynamicInvoke((object)arguments);

			OnFullyCompleted?.Invoke();
		}
	}
}
