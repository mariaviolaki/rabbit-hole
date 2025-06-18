using Dialogue;
using System;
using System.Collections;
using UI;

namespace Commands
{
	public class DialogueCMD : DialogueCommand
	{
		static VisualNovelUI visualNovelUI;

		public new static void Register(CommandManager commandManager)
		{
			visualNovelUI = commandManager.Dialogue.UI;

			CommandBank bank = commandManager.GetBank(CommandManager.MainBankName);

			// Show UI
			bank.AddCommand("ShowVN", new Func<DialogueCommandArguments, IEnumerator>(ShowVN), CommandSkipType.Transition);
			bank.AddCommand("ShowDialogue", new Func<DialogueCommandArguments, IEnumerator>(ShowDialogue), CommandSkipType.Transition);

			// Hide UI
			bank.AddCommand("HideVN", new Func<DialogueCommandArguments, IEnumerator>(HideVN), CommandSkipType.Transition);
			bank.AddCommand("HideDialogue", new Func<DialogueCommandArguments, IEnumerator>(HideDialogue), CommandSkipType.Transition);
		}


		/***** Show Dialogue UI *****/

		static IEnumerator ShowVN(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			yield return visualNovelUI.Show(isImmediate, fadeSpeed);
		}

		static IEnumerator ShowDialogue(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			yield return visualNovelUI.ShowDialogue(isImmediate, fadeSpeed);
		}


		/***** Hide Dialogue UI *****/

		static IEnumerator HideVN(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			yield return visualNovelUI.Hide(isImmediate, fadeSpeed);
		}

		static IEnumerator HideDialogue(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			yield return visualNovelUI.HideDialogue(isImmediate, fadeSpeed);
		}
	}
}
