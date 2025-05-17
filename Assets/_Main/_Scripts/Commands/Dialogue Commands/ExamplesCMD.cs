using System;
using System.Collections;
using UnityEngine;

namespace Commands
{
	public class ExamplesCMD : DialogueCommand
	{
		public new static void Register(CommandManager commandManager)
		{
			CommandDirectory directory = commandManager.GetDirectory(CommandManager.MainDirectoryName);

			// Lambda function examples
			directory.AddCommand("LogLambda", new Action(() => Debug.LogWarning("Logging Lambda...")));
			directory.AddCommand("LogLambdaArg", new Action<string>((string arg) => Debug.LogWarning($"Logging Lambda Arg...\n{arg}")));
			directory.AddCommand("LogLambdaArgs", new Action<string[]>((string[] args) => Debug.LogWarning($"Logging Lambda Args...\n{string.Join("\n", args)}")));

			// Defined function examples
			directory.AddCommand("LogFunction", new Action(LogFunction));
			directory.AddCommand("LogFunctionArg", new Action<string>(LogFunctionArg));
			directory.AddCommand("LogFunctionArgs", new Action<string[]>(LogFunctionArgs));

			// Coroutine function examples
			directory.AddCommand("LogCoroutine", new Func<IEnumerator>(LogCoroutine));
			directory.AddCommand("LogCoroutineArg", new Func<string, IEnumerator>(LogCoroutineArg));
			directory.AddCommand("LogCoroutineArgs", new Func<string[], IEnumerator>(LogCoroutineArgs));
		}

		static void LogFunction()
		{
			Debug.LogWarning("Logging Function...");
		}

		static void LogFunctionArg(string arg)
		{
			Debug.LogWarning($"Logging Function Arg...\n{arg}");
		}

		static void LogFunctionArgs(string[] args)
		{
			Debug.LogWarning($"Logging Function Args...\n{string.Join("\n", args)}");
		}

		static IEnumerator LogCoroutine()
		{
			Debug.LogWarning("Logging Coroutine... (1/2)");
			yield return new WaitForSeconds(1);
			Debug.LogWarning("Logging Coroutine... (2/2)");
		}

		static IEnumerator LogCoroutineArg(string arg)
		{
			Debug.LogWarning("Logging Coroutine Arg... (1/2)");
			yield return new WaitForSeconds(1);
			Debug.LogWarning($"{arg} (2/2)");
		}

		static IEnumerator LogCoroutineArgs(string[] args)
		{
			int logCount = args.Length + 1;
			Debug.LogWarning($"Logging Coroutine Args... (1/{logCount})");

			for (int i = 1; i <= args.Length; i++)
			{
				yield return new WaitForSeconds(1);
				Debug.LogWarning($"{args[i-1]} ({i+1}/{logCount})");
			}
		}
	}
}
