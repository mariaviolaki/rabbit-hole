using Logic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Dialogue
{
	public class ScriptValueParser
	{
		const string identifierPattern = @"[a-zA-Z][a-zA-Z0-9]*";
		const string variableGroupName = "var";
		const string tagGroupName = "tag";
		static readonly string variablePattern = $@"(?<{variableGroupName}>\{{\s*\${identifierPattern}(?:\s*\.\s*{identifierPattern})*\s*\}})";
		static readonly string tagPattern = $@"(?<{tagGroupName}>\<\s*{identifierPattern}\s*\>)";
		static readonly string pattern = $@"{variablePattern}|{tagPattern}";
		static readonly Regex regex = new(pattern);

		readonly ScriptVariableManager variableManager;
		readonly DialogueTagBankSO tagBank;

		public ScriptValueParser(DialogueSystem dialogueSystem)
		{
			variableManager = dialogueSystem.VariableManager;
			tagBank = dialogueSystem.TagBank;
		}

		public string Parse(string text)
		{
			return regex.Replace(text, match =>
			{
				if (match.Groups[variableGroupName].Success)
				{
					string variableName = match.Value.Replace(" ", "");
					variableName = variableName.Substring(2, variableName.Length - 3);

					object variableValue = variableManager.Get(variableName);
					return variableValue != null ? variableValue.ToString() : match.Value;
				}
				else if (match.Groups[tagGroupName].Success)
				{
					string tagName = match.Value.Replace(" ", "");
					tagName = tagName.Substring(1, tagName.Length - 2);

					if (!tagBank.Tags.TryGetValue(tagName, out DialogueTag dialogueTag)) return match.Value;

					return dialogueTag.CurrentValue();
				}
				return match.Value;
			});
		}
	}
}
