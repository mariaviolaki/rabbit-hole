using System;
using System.Collections.Generic;
using System.Globalization;

namespace Variables
{
	public class ScriptVariableManager
	{
		public const char VariablePrefix = '$';
		const string identifierPattern = @"[a-zA-Z][a-zA-Z0-9]*";
		public static readonly string VariablePattern = $@"\{VariablePrefix}{identifierPattern}(?:\s*\.\s*{identifierPattern})*";

		const string DefaultBankName = "Default";
		const char BankSeparator = '.';

		readonly Dictionary<string, ScriptVariableBank> variableBanks = new(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, ScriptVariableBank> VariableBanks => variableBanks;

		public object Get(string name)
		{
			(string bankName, string variableName) = ParseVariableName(name);

			if (!variableBanks.TryGetValue(bankName, out ScriptVariableBank variableBank)) return null;

			return variableBank.Get(variableName);
		}

		public void Set(string name, object value, Func<object> getter = null, Action<object> setter = null)
		{
			(string bankName, string variableName) = ParseVariableName(name);

			if (variableBanks.TryGetValue(bankName, out ScriptVariableBank variableBank))
			{
				variableBank.Set(variableName, value, getter, setter);
			}
			else
			{
				variableBank = new ScriptVariableBank();
				variableBank.Set(variableName, value, getter, setter);
				variableBanks.Add(bankName, variableBank);
			}
		}

		public void SetTyped(string name, string value, ScriptVariableDataType dataType, Func<object> getter = null, Action<object> setter = null)
		{
			if (dataType == ScriptVariableDataType.Int)
			{
				int.TryParse(value, out int intValue);
				Set(name, intValue, getter, setter);
			}
			else if (dataType == ScriptVariableDataType.Float)
			{
				float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue);
				Set(name, floatValue, getter, setter);
			}
			else if (dataType == ScriptVariableDataType.Bool)
			{
				bool.TryParse(value, out bool boolValue);
				Set(name, boolValue, getter, setter);
			}
			else
			{
				Set(name, value, getter, setter);
			}
		}

		public void Remove(string name)
		{
			(string bankName, string variableName) = ParseVariableName(name);
			if (!variableBanks.TryGetValue(bankName, out ScriptVariableBank variableBank)) return;

			variableBank.Remove(variableName);
		}

		public void RemoveAll()
		{
			foreach (ScriptVariableBank bank in variableBanks.Values)
			{
				bank.RemoveAll();
			}
		}

		public void RemoveAllBanks()
		{
			variableBanks.Clear();
		}

		(string bankName, string variableName) ParseVariableName(string name)
		{
			int separatorIndex = name.IndexOf(BankSeparator);
			string bankName = separatorIndex == -1 ? DefaultBankName : name.Substring(0, separatorIndex);
			string variableName = separatorIndex == -1 ? name : name.Substring(separatorIndex + 1);

			return (bankName, variableName);
		}
	}
}
