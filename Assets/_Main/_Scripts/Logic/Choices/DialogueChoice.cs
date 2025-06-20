using System.Collections.Generic;

namespace Logic
{
	public class DialogueChoice
	{
		readonly int index;
		readonly string text;
		readonly List<string> dialogueLines = new();

		public int Index => index;
		public string Text => text;
		public List<string> DialogueLines => dialogueLines;

		public DialogueChoice(int index, string text)
		{
			this.index = index;
			this.text = text;
		}
	}
}
