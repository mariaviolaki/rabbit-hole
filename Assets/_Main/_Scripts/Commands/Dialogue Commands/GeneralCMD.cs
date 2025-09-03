using Dialogue;
using System;
using System.Collections;

namespace Commands
{
	public class GeneralCMD : DialogueCommand
	{
		static CommandManager commandManager;
		static DialogueManager dialogueManager;
		static GameManager gameManager;

		public new static void Register(CommandManager manager)
		{
			commandManager = manager;
			gameManager = manager.Game;
			dialogueManager = manager.Dialogue;

			CommandBank bank = manager.GetBank(CommandManager.MainBankName);

			bank.AddCommand("Wait", new Func<DialogueCommandArguments, CommandProcessBase>(Wait));
			bank.AddCommand("Autosave", new Func<DialogueCommandArguments, CommandProcessBase>(Autosave));
		}

		static CommandProcessBase Wait(DialogueCommandArguments args)
		{
			float time = args.Get(0, "time", 1f);

			IEnumerator process() => dialogueManager.Wait(time);
			return new CoroutineCommandProcess(commandManager, process, true);
		}

		static CommandProcessBase Autosave(DialogueCommandArguments args)
		{
			void action() => gameManager.StateManager.Autosave();
			return new ActionCommandProcess(action);
		}
	}
}
