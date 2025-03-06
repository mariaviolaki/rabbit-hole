using System;
using System.Collections;
using UnityEngine;

namespace DialogueCommands
{
	public class ExamplesCMD : DialogueCommand
	{
		public new static void Register(CommandDirectory commandDirectory)
		{
			// Lambda function examples
			commandDirectory.AddCommand("LogLambda", new Action(() => Debug.Log("Logging Lambda...")));
			commandDirectory.AddCommand("LogLambdaArg", new Action<string>((string arg) => Debug.Log($"Logging Lambda Arg...\n{arg}")));
			commandDirectory.AddCommand("LogLambdaArgs", new Action<string[]>((string[] args) => Debug.Log($"Logging Lambda Args...\n{string.Join("\n", args)}")));

			// Defined function examples
			commandDirectory.AddCommand("LogFunction", new Action(LogFunction));
			commandDirectory.AddCommand("LogFunctionArg", new Action<string>(LogFunctionArg));
			commandDirectory.AddCommand("LogFunctionArgs", new Action<string[]>(LogFunctionArgs));

			// Coroutine function examples
			commandDirectory.AddCommand("LogCoroutine", new Func<IEnumerator>(LogCoroutine));
			commandDirectory.AddCommand("LogCoroutineArg", new Func<string, IEnumerator>(LogCoroutineArg));
			commandDirectory.AddCommand("LogCoroutineArgs", new Func<string[], IEnumerator>(LogCoroutineArgs));
		}

		static void LogFunction()
		{
			Debug.Log("Logging Function...");
		}

		static void LogFunctionArg(string arg)
		{
			Debug.Log($"Logging Function Arg...\n{arg}");
		}

		static void LogFunctionArgs(string[] args)
		{
			Debug.Log($"Logging Function Args...\n{string.Join("\n", args)}");
		}

		static IEnumerator LogCoroutine()
		{
			Debug.Log("Logging Coroutine... (1/2)");
			yield return new WaitForSeconds(1);
			Debug.Log("Logging Coroutine... (2/2)");
		}

		static IEnumerator LogCoroutineArg(string arg)
		{
			Debug.Log("Logging Coroutine Arg... (1/2)");
			yield return new WaitForSeconds(1);
			Debug.Log($"{arg} (2/2)");
		}

		static IEnumerator LogCoroutineArgs(string[] args)
		{
			int logCount = args.Length + 1;
			Debug.Log($"Logging Coroutine Args... (1/{logCount})");

			for (int i = 1; i <= args.Length; i++)
			{
				yield return new WaitForSeconds(1);
				Debug.Log($"{args[i-1]} ({i+1}/{logCount})");
			}
		}
	}
}
