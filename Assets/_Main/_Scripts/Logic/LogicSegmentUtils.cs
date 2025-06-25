using Dialogue;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Variables;

namespace Logic
{
	public class LogicSegmentUtils
	{
		public const char BlockStartDelimiter = '{';
		public const char BlockEndDelimiter = '}';
		static readonly string CalculationOperatorPattern = @"(<=|>=|==|!=|&&|\|\||<|>|\+|-|\*|/|%)";
		static readonly string AssignmentOperatorPattern = @"(\+=|-=|\*=|/=|=)";
		static readonly string AssignmentPattern = @$"^\s*{ScriptVariableManager.VariablePattern}\s*{AssignmentOperatorPattern}";
		public static readonly Regex AssignmentRegex = new(AssignmentPattern);
		public static readonly Regex AssignmentOperatorRegex = new(AssignmentOperatorPattern);
		public static readonly Regex CalculationOperatorRegex = new(CalculationOperatorPattern);

		static readonly string[][] OrderedOperators = new string[][]
		{
			new[] { "*", "/", "%" },
			new[] { "+", "-" },
			new[] { "<", "<=", ">=" , ">" },
			new[] { "==", "!=" },
			new[] { "&&" },
			new[] { "||" }
		};

		readonly ScriptValueParser scriptValueParser;

		public LogicSegmentUtils(DialogueSystem dialogueSystem)
		{
			scriptValueParser = dialogueSystem.Reader.ValueParser;
		}

		public LogicBlock ParseBlock(DialogueBlock dialogueBlock, int progressStartIndex)
		{
			List<string> rawLines = dialogueBlock.Lines;
			int progress = progressStartIndex;
			int depth = 0; // start outside of any blocks

			int startIndex = -1;
			int endIndex = -1;
			List<string> lines = new();

			while (progress < rawLines.Count)
			{
				string rawLine = rawLines[progress].Trim();

				if (rawLine.StartsWith(BlockStartDelimiter))
				{
					depth++;

					if (depth == 1)
					{
						startIndex = progress;
						progress++;
						continue;
					}
				}
				else if (rawLine.StartsWith(BlockEndDelimiter))
				{
					if (depth == 1)
					{
						endIndex = progress;
						break;
					}

					depth--;
				}

				if (depth > 0)
					lines.Add(rawLine);

				progress++;
			}

			if (endIndex == -1) return null;

			int fileStartIndex = dialogueBlock.FileStartIndex + startIndex + 1; // skip the block start identifier
			int fileEndIndex = dialogueBlock.FileStartIndex + endIndex - 1; // don't count the block end identifier
			return new LogicBlock(startIndex, endIndex, lines, dialogueBlock.FilePath, fileStartIndex, fileEndIndex);
		}

		public string EvaluateExpression(string valueString)
		{
			if (string.IsNullOrWhiteSpace(valueString)) return string.Empty;

			// Replace any tags and variables before parsing for operators
			valueString = scriptValueParser.ParseLogic(valueString);

			// Split the string into operators and operands
			MatchCollection operatorMatches = CalculationOperatorRegex.Matches(valueString);
			if (operatorMatches.Count == 0) return EvaluateStringValue(valueString);

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

				valueOperators.Add(valueOperator);
				valueOperands.Add(valueOperand);
			}

			// Evaluate first all operators of higher precedence - and only then move lower
			for (int groupIndex = 0; groupIndex < OrderedOperators.Length; groupIndex++)
			{
				// For the current precedence group, check if any of the operations can be evaluated from left to right
				for (int opIndex = 0; opIndex < valueOperators.Count;)
				{
					string[] operatorGroup = OrderedOperators[groupIndex];
					string expressionOp = valueOperators[opIndex];
					if (!operatorGroup.Contains(expressionOp))
					{
						// For the same operator group, check the next operator in the expression
						opIndex++;
						continue;
					}

					// Save the new value in the right operand and shrink the list from the left
					valueOperands[opIndex + 1] = CalculateValue(valueOperands[opIndex], expressionOp, valueOperands[opIndex + 1]);
					valueOperators.RemoveAt(opIndex);
					valueOperands.RemoveAt(opIndex);

					// Don't increment index - The next operator and operand have shifted into the same index
				}
			}

			return valueOperands[0];
		}

		public string CalculateValue(string leftOperand, string middleOperator, string rightOperand)
		{
			// Convert any tags and variables to their assigned values first
			leftOperand = EvaluateStringValue(leftOperand);
			rightOperand = EvaluateStringValue(rightOperand);

			bool isLeftInvalid = string.IsNullOrWhiteSpace(leftOperand);
			bool isRightInvalid = string.IsNullOrWhiteSpace(rightOperand);

			if (isLeftInvalid && isRightInvalid) return string.Empty;

			bool isLeftNumeric = double.TryParse(leftOperand, NumberStyles.Float, CultureInfo.InvariantCulture, out double leftDouble);
			bool isRightNumeric = double.TryParse(rightOperand, NumberStyles.Float, CultureInfo.InvariantCulture, out double rightDouble);
			bool isLeftBool = bool.TryParse(leftOperand, out bool leftBool);
			bool isRightBool = bool.TryParse(rightOperand, out bool rightBool);

			if ((isLeftNumeric && isRightNumeric) || (isLeftNumeric && isRightInvalid) || (isRightNumeric && isLeftInvalid))
			{
				// Define the default values if one of the operands is invalid
				leftDouble = isLeftNumeric ? leftDouble : 0;
				rightDouble = isRightNumeric ? rightDouble : 0;

				if (middleOperator == "/" && rightDouble == 0)
				{
					Debug.LogWarning($"Unable to divide {leftOperand} by zero.");
					return "0";
				}

				// Check for available operations between floats and integers
				return middleOperator switch
				{
					"+" => (leftDouble + rightDouble).ToString(CultureInfo.InvariantCulture),
					"-" => (leftDouble - rightDouble).ToString(CultureInfo.InvariantCulture),
					"*" => (leftDouble * rightDouble).ToString(CultureInfo.InvariantCulture),
					"/" => (leftDouble / rightDouble).ToString(CultureInfo.InvariantCulture),
					"%" => (leftDouble % rightDouble).ToString(CultureInfo.InvariantCulture),
					"<" => (leftDouble < rightDouble).ToString(CultureInfo.InvariantCulture),
					"<=" => (leftDouble <= rightDouble).ToString(CultureInfo.InvariantCulture),
					">" => (leftDouble > rightDouble).ToString(CultureInfo.InvariantCulture),
					">=" => (leftDouble >= rightDouble).ToString(CultureInfo.InvariantCulture),
					"==" => (leftDouble == rightDouble).ToString(CultureInfo.InvariantCulture),
					"!=" => (leftDouble != rightDouble).ToString(CultureInfo.InvariantCulture),
					_ => "0"
				};
			}
			else if ((isLeftBool && isRightBool) || (isLeftBool && isRightInvalid) || (isRightBool && isLeftInvalid))
			{
				// Define the default values if one of the operands is invalid
				leftBool = isLeftBool ? leftBool : rightBool;
				rightBool = isRightBool ? rightBool : leftBool;

				// Check for available operations between booleans
				return middleOperator switch
				{
					"==" => (leftBool == rightBool).ToString(),
					"!=" => (leftBool != rightBool).ToString(),
					"&&" => (leftBool && rightBool).ToString(),
					"||" => (leftBool || rightBool).ToString(),
					_ => bool.FalseString
				};
			}
			else
			{
				// Check for available operations between strings
				string leftString = RemoveStringQuotes(leftOperand);
				string rightString = RemoveStringQuotes(rightOperand);
				return middleOperator switch
				{
					"+" => leftString + rightString,
					"==" => (leftString == rightString).ToString(),
					"!=" => (leftString != rightString).ToString(),
					_ => string.Empty
				};
			}
		}

		public string EvaluateStringValue(string rawValue)
		{
			if (string.IsNullOrWhiteSpace(rawValue)) return string.Empty;

			// If the value is preceded by a negation, remove it before continue
			rawValue = rawValue.TrimStart();
			string baseValue = rawValue.TrimStart('!');
			bool isNegated = (rawValue.Length - baseValue.Length) % 2 == 1;

			// Trim and convert any variables or tags to their values
			baseValue = scriptValueParser.ParseLogic(baseValue.Trim());

			// Evaluate the negation in the end if this is a valid boolean
			if (isNegated && bool.TryParse(baseValue, out bool boolValue))
				baseValue = (!boolValue).ToString();

			return baseValue;
		}

		public string RemoveStringQuotes(string stringValue)
		{
			if (string.IsNullOrEmpty(stringValue)) return string.Empty;

			if (stringValue.StartsWith("\"") && stringValue.EndsWith("\"") && stringValue.Length >= 2)
				stringValue = stringValue.Substring(1, stringValue.Length - 2);

			return stringValue;
		}
	}
}
