using Dialogue;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Logic
{
	public class AssignmentLogicSegment : LogicSegmentBase
	{
		readonly ScriptVariableManager variableManager;
		readonly DialogueTagManager tagManager;

		static readonly string VariablePattern = @"\$[a-zA-Z][a-zA-Z0-9]*";
		static readonly string CalculationOperatorPattern = @"(\+|-|\*|/)";
		static readonly string AssignmentOperatorPattern = @"(\+=|-=|\*=|/=|=)";
		static readonly string AssignmentPattern = @$"^\s*{VariablePattern}\s*{AssignmentOperatorPattern}";
		static readonly Regex assignmentRegex = new(AssignmentPattern);
		static readonly Regex assignmentOperatorRegex = new(AssignmentOperatorPattern);
		static readonly Regex calculationOperatorRegex = new(CalculationOperatorPattern);

		public static new bool Matches(string rawLine) => assignmentRegex.Match(rawLine).Success;

		public AssignmentLogicSegment(DialogueSystem dialogueSystem, string rawLine) : base(dialogueSystem, rawLine)
		{
			variableManager = dialogueSystem.VariableManager;
			tagManager = dialogueSystem.TagManager;
			ParseAssignment(rawLine);
		}

		public override IEnumerator Execute()
		{
			yield return null;
		}

		void ParseAssignment(string rawLine)
		{
			Match assignmentMatch = assignmentOperatorRegex.Match(rawLine);
			if (!assignmentMatch.Success) return;

			string variableString = rawLine.Substring(0, assignmentMatch.Index).Trim();
			string assignmentOperatorString = assignmentMatch.Value;
			string valueString = rawLine.Substring(assignmentMatch.Index + assignmentMatch.Length).Trim();

			MatchCollection operatorMatches = calculationOperatorRegex.Matches(valueString);

			if (operatorMatches.Count > 0)
				valueString = CalculateAssignmentValue(valueString, operatorMatches);

			AssignValue(variableString, assignmentOperatorString, valueString);
		}

		void AssignValue(string variableString, string assignmentOperator, string valueString)
		{
			string newValue;
			string variableName = variableString.StartsWith(ScriptVariableManager.VariablePrefix)
				? variableString.Substring(1)
				: variableString;
			
			if (assignmentOperator == "=")
			{
				newValue = EvaluateStringValue(valueString);
			}
			else
			{
				string calculationOperator = assignmentOperator.Substring(0, 1);
				newValue = CalculateValue(variableString, calculationOperator, valueString);
			}

			if (bool.TryParse(newValue, out bool parsedBool))
				variableManager.Set(variableName, parsedBool);
			else if (int.TryParse(newValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
				variableManager.Set(variableName, parsedInt);
			else if (float.TryParse(newValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat))
				variableManager.Set(variableName, parsedFloat);
			else
				variableManager.Set(variableName, RemoveStringQuotes(newValue));
		}

		string CalculateAssignmentValue(string valueString, MatchCollection operatorMatches)
		{
			if (operatorMatches.Count == 0) return valueString.Trim();

			List<string> valueOperands = new();
			List<string> valueOperators = new();

			valueOperands.Add(valueString.Substring(0, operatorMatches[0].Index).Trim());

			for (int i = 0; i < operatorMatches.Count; i++)
			{
				Match match = operatorMatches[i];
				Match nextMatch = (i == operatorMatches.Count - 1) ? null : operatorMatches[i + 1];
				int operandIndex = match.Index + match.Length;

				string valueOperator = match.Value;
				string valueOperand = nextMatch == null
					? valueString.Substring(operandIndex).Trim()
					: valueString.Substring(operandIndex, nextMatch.Index - operandIndex).Trim();

				// Process any multiplications and divisions during the first pass
				if (IsMultiplicationOrDivision(valueOperator))
				{
					int lastValueIndex = valueOperands.Count - 1;
					valueOperands[lastValueIndex] = CalculateValue(valueOperands[lastValueIndex], valueOperator, valueOperand);
				}
				else
				{
					valueOperators.Add(valueOperator);
					valueOperands.Add(valueOperand);
				}
			}
			
			// Perform any additions and subtractions in the end
			while (valueOperands.Count > 1 && valueOperators.Count > 0)
			{
				valueOperands[1] = CalculateValue(valueOperands[0], valueOperators[0], valueOperands[1]);
				valueOperands.RemoveAt(0);
				valueOperators.RemoveAt(0);
			}

			return valueOperands[0];
		}

		string CalculateValue(string leftOperand, string calculationOperator, string rightOperand)
		{
			// Convert any tags and variables to their assigned values first
			leftOperand = EvaluateStringValue(leftOperand);
			rightOperand = EvaluateStringValue(rightOperand);

			bool isLeftInvalid = string.IsNullOrWhiteSpace(leftOperand);
			bool isRightInvalid = string.IsNullOrWhiteSpace(rightOperand);

			if (isLeftInvalid && isRightInvalid) return string.Empty;
			if (isLeftInvalid) return RemoveStringQuotes(rightOperand);
			if (isRightInvalid) return RemoveStringQuotes(leftOperand);

			if (double.TryParse(leftOperand, NumberStyles.Float, CultureInfo.InvariantCulture, out double left)
					&& double.TryParse(rightOperand, NumberStyles.Float, CultureInfo.InvariantCulture, out double right))
			{
				if (calculationOperator == "/" && right == 0)
				{
					Debug.LogWarning($"Unable to divide {leftOperand} by zero.");
					return "0";
				}

				return calculationOperator switch
				{
					"+" => (left + right).ToString(CultureInfo.InvariantCulture),
					"-" => (left - right).ToString(CultureInfo.InvariantCulture),
					"*" => (left * right).ToString(CultureInfo.InvariantCulture),
					"/" => (left / right).ToString(CultureInfo.InvariantCulture),
					_ => "0"
				};
			}
			else if (calculationOperator == "+")
			{
				return RemoveStringQuotes(leftOperand) + RemoveStringQuotes(rightOperand);
			}

			return string.Empty;
		}

		string EvaluateStringValue(string rawValue)
		{
			if (string.IsNullOrWhiteSpace(rawValue)) return string.Empty;

			// If the value is preceded by a negation, remove it before continue
			bool isNegated = rawValue.StartsWith('!');
			if (isNegated)
				rawValue = rawValue.Substring(1).Trim();

			// Evaluate <tags>
			if (rawValue.Length >= 3 && rawValue.StartsWith(DialogueTagManager.TagStart) && rawValue.EndsWith(DialogueTagManager.TagEnd))
			{
				string tagName = rawValue.Substring(1, rawValue.Length - 2);
				rawValue = tagManager.GetTagValue(tagName) ?? rawValue;
			}

			// Evaluate $variables
			else if (rawValue.Length >= 2 && rawValue.StartsWith(ScriptVariableManager.VariablePrefix))
			{
				string variableName = rawValue.Substring(1);
				rawValue = variableManager.Get(variableName)?.ToString() ?? rawValue;
			}

			// Evaluate the negation in the end if this is a valid boolean
			if (isNegated && bool.TryParse(rawValue, out bool boolValue))
				rawValue = (!boolValue).ToString();

			return rawValue;
		}

		string RemoveStringQuotes(string stringValue)
		{
			if (string.IsNullOrEmpty(stringValue)) return string.Empty;

			if (stringValue.StartsWith("\"") && stringValue.EndsWith("\"") && stringValue.Length >= 2)
				stringValue = stringValue.Substring(1, stringValue.Length - 2);

			return stringValue;
		}

		bool IsMultiplicationOrDivision(string operatorString) => operatorString == "*" || operatorString == "/";
	}
}
