using Dialogue;
using System;
using System.Collections;

namespace Commands
{
	public class GeneralCMD : DialogueCommand
	{
		static DialogueSystem dialogueSystem;

		public new static void Register(CommandManager commandManager)
		{
			dialogueSystem = commandManager.Dialogue;

			CommandBank bank = commandManager.GetBank(CommandManager.MainBankName);

			bank.AddCommand("Wait", new Func<DialogueCommandArguments, IEnumerator>(Wait));
			bank.AddCommand("Load", new Action<DialogueCommandArguments>(Load));
		}

		static IEnumerator Wait(DialogueCommandArguments args)
		{
			float time = args.Get(0, "time", 1f);

			yield return dialogueSystem.Wait(time);
		}

		static void Load(DialogueCommandArguments args)
		{
			string path = args.Get(0, "path", "");

			dialogueSystem.LoadDialogue(path, Guid.NewGuid());
		}
	}
}
