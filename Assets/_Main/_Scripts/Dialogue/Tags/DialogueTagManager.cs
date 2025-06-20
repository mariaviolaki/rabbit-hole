using System;
using UI;
using UnityEngine;

namespace Dialogue
{
	public class DialogueTagManager
	{
		public const char TagStart = '<';
		public const char TagEnd = '>';
		readonly DialogueTagBankSO tagBank;
		readonly InputPanelUI inputUI;

		public DialogueTagManager(DialogueSystem dialogueSystem)
		{
			tagBank = dialogueSystem.TagBank;
			inputUI = dialogueSystem.UI.InputUI;

			InitTags();
		}

		public string GetTagValue(string tagName)
		{
			if (!tagBank.Tags.TryGetValue(tagName, out DialogueTag dialogueTag)) return null;

			return dialogueTag.CurrentValue();
		}

		void InitTags()
		{
			SetTagValue("input", () => inputUI.LastInput);
		}

		void SetTagValue(string tagName, Func<string> value)
		{
			if (!tagBank.Tags.TryGetValue(tagName, out DialogueTag dialogueTag))
			{
				Debug.LogWarning($"Tag '{tagName}' not found in Tag Bank.");
				return;
			}

			dialogueTag.CurrentValue = value;
		}
	}
}
