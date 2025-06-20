using System;
using System.Collections.Generic;

namespace Logic
{
	public class ScriptVariableManager
	{
		public const char VariablePrefix = '$';
		const string DefaultBankName = "Default";
		const char BankSeparator = '.';

		readonly Dictionary<string, ScriptVariableBank> variableBanks = new(StringComparer.OrdinalIgnoreCase);

		public object Get(string name)
		{
			(string bankName, string variableName) = ParseVariableName(name);

			if (!variableBanks.TryGetValue(bankName, out ScriptVariableBank variableBank)) return null;

			return variableBank.Get(variableName);
		}

		public void Set<T>(string name, T value, Func<T> getter = null, Action<T> setter = null)
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

		(string bankName, string variableName) ParseVariableName(string name)
		{
			int separatorIndex = name.IndexOf(BankSeparator);
			string bankName = separatorIndex == -1 ? DefaultBankName : name.Substring(0, separatorIndex);
			string variableName = separatorIndex == -1 ? name : name.Substring(separatorIndex + 1);

			return (bankName, variableName);
		}
	}
}
