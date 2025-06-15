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
			visualNovelUI = commandManager.GetDialogueSystem().GetVisualNovelUI();

			CommandDirectory directory = commandManager.GetDirectory(CommandManager.DialogueDirectoryName);

			// Show Dialogue UI
			directory.AddCommand("Show", new Func<DialogueCommandArguments, IEnumerator>(Show), CommandSkipType.Transition);
			directory.AddCommand("ShowDialogue", new Func<DialogueCommandArguments, IEnumerator>(ShowDialogue), CommandSkipType.Transition);

			// Hide Dialogue UI
			directory.AddCommand("Hide", new Func<DialogueCommandArguments, IEnumerator>(Hide), CommandSkipType.Transition);
			directory.AddCommand("HideDialogue", new Func<DialogueCommandArguments, IEnumerator>(HideDialogue), CommandSkipType.Transition);
		}


		/***** Show Dialogue UI *****/

		static IEnumerator Show(DialogueCommandArguments args)
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

		static IEnumerator Hide(DialogueCommandArguments args)
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
