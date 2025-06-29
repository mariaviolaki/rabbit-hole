using Dialogue;
using System;
using System.Collections;
using UnityEngine;

namespace Commands
{
	public class CommandProcess
	{
		readonly Guid id;
		readonly string name;
		readonly DialogueCommandArguments arguments;
		readonly Delegate command;
		readonly CommandManager commandManager;
		readonly CommandSkipType skipType;
		readonly bool isBlocking;

		Coroutine commandCoroutine;
		Coroutine skipCommandCoroutine;

		public Guid Id => id;
		public string Name => name;
		public bool IsCompleted => commandCoroutine == null;
		public bool IsSkipCompleted => skipType == CommandSkipType.None || skipCommandCoroutine == null;

		public bool IsBlocking => isBlocking;

		public CommandProcess(Guid id, string name, DialogueCommandArguments arguments, Delegate command, CommandManager commandManager, CommandSkipType skipType, bool isBlocking)
		{
			this.id = id;
			this.name = name;
			this.arguments = arguments;
			this.command = command;
			this.commandManager = commandManager;
			this.isBlocking = isBlocking;
			this.skipType = skipType;
		}

		public void Start()
		{
			commandCoroutine = ExecuteProcess(false);
		}

		public void Stop()
		{
			if (isBlocking || commandCoroutine == null) return;

			commandManager.StopCoroutine(commandCoroutine);
			commandCoroutine = null;

			if (skipType == CommandSkipType.Immediate)
				skipCommandCoroutine = ExecuteImmediate(true);
			else if (skipType == CommandSkipType.Transition)
				skipCommandCoroutine = ExecuteProcess(true);
		}

		Coroutine ExecuteProcess(bool isSkipped)
		{
			if (command is Func<DialogueCommandArguments, Coroutine> coroutineFunc)
			{
				Coroutine coroutine = coroutineFunc(arguments);
				return commandManager.StartCoroutine(CoroutineRunner(coroutine, isSkipped));
			}

			return null;
		}

		Coroutine ExecuteImmediate(bool isSkipped)
		{
			arguments.AddNamedArgument("immediate", "true");

			if (command is Func<DialogueCommandArguments, Coroutine> coroutineFunc)
			{
				Coroutine coroutine = coroutineFunc(arguments);
				return commandManager.StartCoroutine(CoroutineRunner(coroutine, isSkipped));
			}

			return null;
		}

		IEnumerator CoroutineRunner(Coroutine wrappedCoroutine, bool isSkipped)
		{
			if (wrappedCoroutine != null)
				yield return wrappedCoroutine;

			if (isSkipped) skipCommandCoroutine = null;
			else commandCoroutine = null;
		}
	}
}
