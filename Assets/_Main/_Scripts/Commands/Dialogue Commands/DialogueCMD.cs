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
			directory.AddCommand("ShowInstant", new Action<string[]>(ShowInstant));
			directory.AddCommand("Show", new Func<string[], IEnumerator>(Show), new Action<string[]>(ShowInstant));
			directory.AddCommand("ShowDialogueInstant", new Action<string[]>(ShowDialogueInstant));
			directory.AddCommand("ShowDialogue", new Func<string[], IEnumerator>(ShowDialogue), new Action<string[]>(ShowDialogueInstant));

			// Hide Dialogue UI
			directory.AddCommand("HideInstant", new Action<string[]>(HideInstant));
			directory.AddCommand("Hide", new Func<string[], IEnumerator>(Hide), new Action<string[]>(HideInstant));
			directory.AddCommand("HideDialogueInstant", new Action<string[]>(HideDialogueInstant));
			directory.AddCommand("HideDialogue", new Func<string[], IEnumerator>(HideDialogue), new Action<string[]>(HideDialogueInstant));
		}


		/***** Show Dialogue UI *****/

		static void ShowInstant(string[] args)
		{
			visualNovelUI.ShowInstant();
		}

		static IEnumerator Show(string[] args)
		{
			float fadeSpeed = args.Length > 0 ? ParseArgument<float>(args[0]) : 0;

			yield return visualNovelUI.Show(fadeSpeed);
		}

		static void ShowDialogueInstant(string[] args)
		{
			visualNovelUI.ShowDialogueInstant();
		}

		static IEnumerator ShowDialogue(string[] args)
		{
			float fadeSpeed = args.Length > 0 ? ParseArgument<float>(args[0]) : 0;

			yield return visualNovelUI.ShowDialogue(fadeSpeed);
		}


		/***** Hide Dialogue UI *****/

		static void HideInstant(string[] args)
		{
			visualNovelUI.HideInstant();
		}

		static IEnumerator Hide(string[] args)
		{
			float fadeSpeed = args.Length > 0 ? ParseArgument<float>(args[0]) : 0;

			yield return visualNovelUI.Hide(fadeSpeed);
		}

		static void HideDialogueInstant(string[] args)
		{
			visualNovelUI.HideDialogueInstant();
		}

		static IEnumerator HideDialogue(string[] args)
		{
			float fadeSpeed = args.Length > 0 ? ParseArgument<float>(args[0]) : 0;

			yield return visualNovelUI.HideDialogue(fadeSpeed);
		}
	}
}
