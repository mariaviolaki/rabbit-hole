using System.Text.RegularExpressions;
using Variables;

namespace Dialogue
{
	public class ScriptValueParser
	{
		const string identifierPattern = @"[a-zA-Z][a-zA-Z0-9]*";
		const string variableGroupName = "var";
		const string tagGroupName = "tag";
		static readonly string variablePattern = $@"\$(?<{variableGroupName}>{identifierPattern}(?:\s*\.\s*{identifierPattern})*)";
		static readonly string tagPattern = $@"<\s*(?<{tagGroupName}>{identifierPattern})\s*>";
		static readonly string textPattern = $@"\{{\s*{variablePattern}\s*\}}|\{{\s*{tagPattern}\s*\}}";
		static readonly string logicPattern = $@"{variablePattern}|{tagPattern}";
		static readonly Regex textRegex = new(textPattern);
		static readonly Regex logicRegex = new(logicPattern);

		readonly ScriptVariableManager variableManager;
		readonly DialogueTagBankSO tagBank;

		public ScriptValueParser(DialogueSystem dialogueSystem)
		{
			variableManager = dialogueSystem.VariableManager;
			tagBank = dialogueSystem.TagBank;
		}

		public string ParseText(string text)
		{
			return Parse(textRegex, text, false);
		}

		public string ParseLogic(string text)
		{
			return Parse(logicRegex, text, true);
		}

		string Parse(Regex regex, string text, bool isLogic)
		{
			return regex.Replace(text, match =>
			{
				if (match.Groups[variableGroupName].Success)
				{
					// Remove any spaces between the dot separating the variable bank from the variable name
					string variableName = match.Groups[variableGroupName].Value.Replace(" ", "");
					object variableValue = variableManager.Get(variableName);
					if (variableValue != null) return variableValue.ToString();
					else if (isLogic) return string.Empty;
					else return variableName;
				}
				else if (match.Groups[tagGroupName].Success)
				{
					string tagName = match.Groups[tagGroupName].Value;
					if (tagBank.Tags.TryGetValue(tagName, out DialogueTag dialogueTag)) return dialogueTag.CurrentValue;
					else if (isLogic) return string.Empty;
					else return tagName;
				}
				return match.Value;
			});
		}
	}
}
