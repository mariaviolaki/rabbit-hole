using Dialogue;
using System;
using UI;
using UnityEngine;

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
			bank.AddCommand("ShowVN", new Func<DialogueCommandArguments, Coroutine>(ShowVN), CommandSkipType.Transition);
			bank.AddCommand("ShowDialogue", new Func<DialogueCommandArguments, Coroutine>(ShowDialogue), CommandSkipType.Transition);

			// Hide UI
			bank.AddCommand("HideVN", new Func<DialogueCommandArguments, Coroutine>(HideVN), CommandSkipType.Transition);
			bank.AddCommand("HideDialogue", new Func<DialogueCommandArguments, Coroutine>(HideDialogue), CommandSkipType.Transition);
		}


		/***** Show Dialogue UI *****/

		static Coroutine ShowVN(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			return visualNovelUI.Show(isImmediate, fadeSpeed);
		}

		static Coroutine ShowDialogue(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			return visualNovelUI.Dialogue.Show(isImmediate, fadeSpeed);
		}


		/***** Hide Dialogue UI *****/

		static Coroutine HideVN(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			return visualNovelUI.Hide(isImmediate, fadeSpeed);
		}

		static Coroutine HideDialogue(DialogueCommandArguments args)
		{
			bool isImmediate = args.Get(0, "immediate", false);
			float fadeSpeed = args.Get(1, "speed", 0f);

			return visualNovelUI.Dialogue.Hide(isImmediate, fadeSpeed);
		}
	}
}
