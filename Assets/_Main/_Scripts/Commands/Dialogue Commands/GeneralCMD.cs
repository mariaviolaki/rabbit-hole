using Dialogue;
using System;
using System.Collections;
using UnityEngine;

namespace Commands
{
	public class GeneralCMD : DialogueCommand
	{
		static DialogueSystem dialogueSystem;

		public new static void Register(CommandManager commandManager)
		{
			dialogueSystem = commandManager.Dialogue;

			CommandDirectory directory = commandManager.GetDirectory(CommandManager.MainDirectoryName);

			directory.AddCommand("Wait", new Func<DialogueCommandArguments, IEnumerator>(Wait));
			directory.AddCommand("Load", new Func<DialogueCommandArguments, IEnumerator>(Load));
		}

		static IEnumerator Wait(DialogueCommandArguments args)
		{
			float time = args.Get(0, "time", 1f);

			yield return new WaitForSeconds(time);
		}

		static IEnumerator Load(DialogueCommandArguments args)
		{
			string path = args.Get(0, "path", "");

			yield return dialogueSystem.LoadDialogue(path);
		}
	}
}
