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
		readonly MonoBehaviour owner;
		readonly bool isUnskippable;

		Coroutine commandCoroutine;
		Coroutine skipCommandCoroutine;

		public Action OnFullyCompleted;

		public string Name => name;
		public bool IsCompleted => commandCoroutine == null;

		public CommandProcess(string name, string[] arguments, Delegate command, Delegate skipCommand, MonoBehaviour owner, bool isUnskippable)
		{
			this.name = name;
			this.arguments = arguments;
			this.command = command;
			this.skipCommand = skipCommand;
			this.owner = owner;
			this.isUnskippable = isUnskippable;
		}

		public void Start()
		{
			commandCoroutine = owner.StartCoroutine(WaitForExecution(command, arguments));
		}

		public void Stop()
		{
			if (isUnskippable || commandCoroutine == null) return;

			owner.StopCoroutine(commandCoroutine);
			commandCoroutine = null;

			if (skipCommand == null)
				OnFullyCompleted?.Invoke();
			else
				skipCommandCoroutine = owner.StartCoroutine(WaitForExecution(skipCommand, arguments));
		}

		IEnumerator WaitForExecution(Delegate command, string[] arguments)
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
	}
}
