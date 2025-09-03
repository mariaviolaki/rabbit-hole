using System.Text.RegularExpressions;
using Variables;

namespace Dialogue
{
	public static class ScriptVariableUtilities
	{
		const string identifierPattern = @"[a-zA-Z_][a-zA-Z0-9_]*";
		const string variableGroupName = "var";
		static readonly string variablePattern = $@"\$(?<{variableGroupName}>{identifierPattern}(?:\s*\.\s*{identifierPattern})*)";
		static readonly Regex textRegex = new($@"\{{\s*{variablePattern}\s*\}}", RegexOptions.Compiled);
		static readonly Regex logicRegex = new($@"{variablePattern}", RegexOptions.Compiled);

		public static string ParseText(string text, VariableManager variableManager)
		{
			return Parse(variableManager, textRegex, text, false);
		}

		public static string ParseLogic(string text, VariableManager variableManager)
		{
			return Parse(variableManager, logicRegex, text, true);
		}

		static string Parse(VariableManager variableManager, Regex regex, string text, bool isLogic)
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
				return match.Value;
			});
		}
	}
}
