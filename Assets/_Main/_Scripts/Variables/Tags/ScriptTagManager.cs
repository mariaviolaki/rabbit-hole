using Dialogue;
using UnityEngine;

namespace Variables
{
	public class ScriptTagManager
	{
		public const char TagStart = '<';
		public const char TagEnd = '>';
		readonly DialogueTagBankSO tagBank;

		public DialogueTagBankSO TagBank => tagBank;

		public ScriptTagManager(DialogueManager dialogueManager)
		{
			tagBank = dialogueManager.TagBank;
		}

		public string GetTagValue(string tagName)
		{
			if (!tagBank.Tags.TryGetValue(tagName, out DialogueTag dialogueTag)) return null;

			return dialogueTag.CurrentValue;
		}

		public void SetTagValue(string tagName, string value)
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
