using System;
using System.Collections;
using UnityEngine;

namespace Commands
{
	public class GeneralCMD : DialogueCommand
	{
		public new static void Register(CommandDirectory commandDirectory)
		{
			commandDirectory.AddCommand("Wait", new Func<string, IEnumerator>(Wait));
		}

		static IEnumerator Wait(string arg)
		{
			float time = ParseArgument<float>(arg);
			yield return new WaitForSeconds(time);
		}
	}
}
