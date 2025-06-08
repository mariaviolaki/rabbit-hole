using System;
using System.Collections;

namespace Commands
{
	public class DialogueCMD : DialogueCommand
	{
		static DialogueUI dialogueUI;

		public new static void Register(CommandManager commandManager)
		{
			dialogueUI = commandManager.GetDialogueUI();

			CommandDirectory directory = commandManager.GetDirectory(CommandManager.DialogueDirectoryName);

			directory.AddCommand("ShowDialogueBox", new Func<string[], IEnumerator>(ShowDialogueBox));
			directory.AddCommand("HideDialogueBox", new Func<string[], IEnumerator>(HideDialogueBox));
		}

		static IEnumerator ShowDialogueBox(string[] args)
		{
			yield return dialogueUI.ShowDialogueBox();
		}

		static IEnumerator HideDialogueBox(string[] args)
		{
			yield return dialogueUI.HideDialogueBox();
		}
	}
}
