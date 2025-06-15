using System;
using System.Text.RegularExpressions;
using UI;
using UnityEngine;

namespace Dialogue
{
	public class DialogueTagManager
	{
		const string tagPattern = @"<[a-zA-Z][a-zA-Z0-9]*>";
		static readonly Regex regex = new Regex(tagPattern);

		readonly DialogueTagDirectorySO tagDirectory;
		readonly InputPanelUI inputUI;

		public DialogueTagManager(DialogueSystem dialogueSystem)
		{
			tagDirectory = dialogueSystem.GetTagDirectory();
			inputUI = dialogueSystem.GetVisualNovelUI().InputUI;

			InitTags();
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

		public string GetTagValue(string tagName)
		{
			if (!tagDirectory.Tags.TryGetValue(tagName, out DialogueTag dialogueTag)) return null;

			return dialogueTag.CurrentValue();
		}

		void InitTags()
		{
			SetTagValue("input", () => inputUI.LastInput);
		}

		void SetTagValue(string tagName, Func<string> value)
		{
			string formattedTagName = $"<{tagName}>";
			if (!tagDirectory.Tags.TryGetValue(formattedTagName, out DialogueTag dialogueTag))
			{
				Debug.LogWarning($"Tag '{tagName}' not found in Tag Directory.");
				return;
			}

			dialogueTag.CurrentValue = value;
		}

		string ReplaceFirst(string text, string tag, string tagValue)
		{
			int tagIndex = text.IndexOf(tag);
			if (tagIndex == -1) return text;

			return text.Substring(0, tagIndex) + tagValue + text.Substring(tagIndex + tag.Length);
		}
	}
}
