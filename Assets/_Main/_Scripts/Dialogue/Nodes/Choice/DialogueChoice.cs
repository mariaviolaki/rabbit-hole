namespace Dialogue
{
	public class DialogueChoice
	{
		readonly int index;
		readonly string text;

		public int Index => index;
		public string Text => text;

		public DialogueChoice(int index, string text)
		{
			this.index = index;
			this.text = text;
		}
	}
}
