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

		Coroutine commandCoroutine;

		public Guid Id => id;
		public string Name => name;
		public bool IsCompleted => commandCoroutine == null;
		public bool IsBlocking => skipType == CommandSkipType.None;

		public CommandProcess(Guid id, string name, DialogueCommandArguments arguments, Delegate command, CommandManager commandManager, CommandSkipType skipType)
		{
			this.id = id;
			this.name = name;
			this.arguments = arguments;
			this.command = command;
			this.commandManager = commandManager;
			this.skipType = skipType;
		}

		public void Start()
		{
			commandCoroutine = ExecuteProcess();
		}

		public void Stop()
		{
			if (skipType == CommandSkipType.None || commandCoroutine == null) return;

			commandManager.StopCoroutine(commandCoroutine);
			commandCoroutine = null;

			if (skipType == CommandSkipType.Immediate)
				ExecuteImmediate();
			else if (skipType == CommandSkipType.Transition)
				ExecuteProcess();
		}

		Coroutine ExecuteProcess()
		{
			if (command is Func<DialogueCommandArguments, Coroutine> coroutineFunc)
			{
				Coroutine coroutine = coroutineFunc(arguments);
				return commandManager.StartCoroutine(CoroutineRunner(coroutine));
			}

			return null;
		}

		Coroutine ExecuteImmediate()
		{
			arguments.AddNamedArgument("immediate", "true");

			if (command is Func<DialogueCommandArguments, Coroutine> coroutineFunc)
			{
				Coroutine coroutine = coroutineFunc(arguments);
				return commandManager.StartCoroutine(CoroutineRunner(coroutine));
			}
			else if (command is Func<DialogueCommandArguments, IEnumerator> ienumeratorFunc)
			{
				return commandManager.StartCoroutine(ienumeratorFunc(arguments));
			}

			return null;
		}

		IEnumerator CoroutineRunner(Coroutine wrappedCoroutine)
		{
			if (wrappedCoroutine != null)
				yield return wrappedCoroutine;

			commandCoroutine = null;
		}
	}
}
