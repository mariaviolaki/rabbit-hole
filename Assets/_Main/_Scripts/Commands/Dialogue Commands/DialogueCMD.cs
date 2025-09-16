using Dialogue;
using System;
using System.Collections;
using UI;

namespace Commands
{
	public class DialogueCMD : DialogueCommand
	{
		static CommandManager commandManager;
		static VisualNovelUI visualNovelUI;

		public new static void Register(CommandManager manager)
		{
			commandManager = manager;
			visualNovelUI = manager.VN.UI;

			CommandBank bank = manager.GetBank(CommandManager.MainBankName);

			// Show UI
			bank.AddCommand("ShowDialogue", new Func<DialogueCommandArguments, CommandProcessBase>(ShowDialogue));

			// Hide UI
			bank.AddCommand("HideDialogue", new Func<DialogueCommandArguments, CommandProcessBase>(HideDialogue));
		}


		/***** Show Dialogue UI *****/

		static CommandProcessBase ShowDialogue(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			IEnumerator process() => visualNovelUI.Dialogue.SetVisible(isImmediate, fadeSpeed);
			return new CoroutineCommandProcess(commandManager, process, true);
		}


		/***** Hide Dialogue UI *****/

		static CommandProcessBase HideDialogue(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			IEnumerator process() => visualNovelUI.Dialogue.SetHidden(isImmediate, fadeSpeed);
			return new CoroutineCommandProcess(commandManager, process, true);
		}
	}
}
