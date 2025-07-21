using Dialogue;
using System.Globalization;
using System.Text.RegularExpressions;
using Variables;

namespace Logic
{
	public class AssignmentLogicSegment : NonBlockingLogicSegmentBase
	{
		public static new bool Matches(string rawLine) => LogicSegmentUtils.AssignmentRegex.Match(rawLine).Success;

		readonly ScriptVariableManager variableManager;
		readonly LogicSegmentUtils logicSegmentUtils;

		public AssignmentLogicSegment(DialogueManager dialogueManager, string rawLine) : base(dialogueManager, rawLine)
		{
			variableManager = dialogueManager.VariableManager;
			logicSegmentUtils = new(dialogueManager);
		}

		public override void Execute()
		{
			ParseAssignment();
		}

		void ParseAssignment()
		{
			Match assignmentMatch = LogicSegmentUtils.AssignmentOperatorRegex.Match(rawLine);
			if (!assignmentMatch.Success) return;

			string variableString = rawLine.Substring(0, assignmentMatch.Index).Trim();
			string assignmentOperatorString = assignmentMatch.Value;
			string valueString = rawLine.Substring(assignmentMatch.Index + assignmentMatch.Length).Trim();
			string evaluatedValueString = logicSegmentUtils.EvaluateExpression(valueString);

			AssignValue(variableString, assignmentOperatorString, evaluatedValueString);
		}

		void AssignValue(string variableString, string assignmentOperator, string valueString)
		{
			string newValue;
			string variableName = variableString.StartsWith(ScriptVariableManager.VariablePrefix)
				? variableString.Substring(1)
				: variableString;

			if (assignmentOperator == "=")
			{
				newValue = logicSegmentUtils.EvaluateStringValue(valueString);
			}
			else
			{
				string calculationOperator = assignmentOperator.Substring(0, 1);
				newValue = logicSegmentUtils.CalculateValue(variableString, calculationOperator, valueString);
			}

			if (bool.TryParse(newValue, out bool parsedBool))
				variableManager.Set(variableName, parsedBool);
			else if (int.TryParse(newValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
				variableManager.Set(variableName, parsedInt);
			else if (float.TryParse(newValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat))
				variableManager.Set(variableName, parsedFloat);
			else
				variableManager.Set(variableName, logicSegmentUtils.RemoveStringQuotes(newValue));
		}
	}
}
