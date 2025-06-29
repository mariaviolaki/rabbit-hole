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

			CommandBank bank = commandManager.GetBank(CommandManager.MainBankName);

			bank.AddCommand("Wait", new Func<DialogueCommandArguments, Coroutine>(Wait));
			bank.AddCommand("Load", new Func<DialogueCommandArguments, Coroutine>(Load));
		}

		static Coroutine Wait(DialogueCommandArguments args)
		{
			float time = args.Get(0, "time", 1f);

			return dialogueSystem.Wait(time);
		}

		static Coroutine Load(DialogueCommandArguments args)
		{
			string path = args.Get(0, "path", "");

			return dialogueSystem.ReplaceDialogue(path);
		}
	}
}
