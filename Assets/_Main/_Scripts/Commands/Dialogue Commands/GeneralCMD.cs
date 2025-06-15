using Dialogue;
using System;
using System.Collections;
using UnityEngine;

namespace Commands
{
	public class GeneralCMD : DialogueCommand
	{
		public new static void Register(CommandManager commandManager)
		{
			CommandDirectory directory = commandManager.GetDirectory(CommandManager.MainDirectoryName);

			directory.AddCommand("Wait", new Func<DialogueCommandArguments, IEnumerator>(Wait));
		}

		static IEnumerator Wait(DialogueCommandArguments args)
		{
			float time = args.Get(0, "time", 1f);

			yield return new WaitForSeconds(time);
		}
	}
}
