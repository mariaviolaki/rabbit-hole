using System.Text.RegularExpressions;

namespace Dialogue
{
	public class DialogueTagManager
	{
		const string tagPattern = @"<[a-zA-Z][a-zA-Z0-9]*>";
		static readonly Regex regex = new Regex(tagPattern);

		readonly DialogueTagDirectorySO tagDirectory;

		public DialogueTagManager(DialogueTagDirectorySO tagDirectory)
		{
			this.tagDirectory = tagDirectory;
		}

		public string Parse(string text)
		{
			MatchCollection matches = regex.Matches(text);

			foreach (Match match in matches)
			{
				if (tagDirectory.Tags.TryGetValue(match.Value, out DialogueTag dialogueTag))
				{
					text = ReplaceFirst(text, match.Value, dialogueTag.CurrentValue());
				}
			}

			return text;
		}

		public string GetTagValue(string text)
		{
			if (!tagDirectory.Tags.TryGetValue(text, out DialogueTag dialogueTag)) return null;

			return dialogueTag.CurrentValue();
		}

		string ReplaceFirst(string text, string tag, string tagValue)
		{
			int tagIndex = text.IndexOf(tag);
			if (tagIndex == -1) return text;

			return text.Substring(0, tagIndex) + tagValue + text.Substring(tagIndex + tag.Length);
		}
	}
}
