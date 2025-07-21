using Dialogue;
using System;
using System.Collections;

namespace Commands
{
	public class GeneralCMD : DialogueCommand
	{
		static DialogueManager dialogueManager;
		static GameManager gameManager;

		public new static void Register(CommandManager commandManager)
		{
			gameManager = commandManager.Game;
			dialogueManager = commandManager.Dialogue;

			CommandBank bank = commandManager.GetBank(CommandManager.MainBankName);

			bank.AddCommand("Wait", new Func<DialogueCommandArguments, IEnumerator>(Wait));
			bank.AddCommand("Load", new Action<DialogueCommandArguments>(Load));
			bank.AddCommand("Autosave", new Action<DialogueCommandArguments>(Autosave));
		}

		static IEnumerator Wait(DialogueCommandArguments args)
		{
			float time = args.Get(0, "time", 1f);

			yield return dialogueManager.Wait(time);
		}

		static void Load(DialogueCommandArguments args)
		{
			string path = args.Get(0, "path", "");

			dialogueManager.LoadDialogue(path);
		}

		static void Autosave(DialogueCommandArguments args)
		{
			gameManager.Autosave();
		}
	}
}
