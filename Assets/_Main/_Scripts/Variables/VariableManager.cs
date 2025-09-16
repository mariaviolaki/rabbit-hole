using Game;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Variables
{
	public class VariableManager : MonoBehaviour
	{
		[SerializeField] GameProgressManager gameProgressManager;

		public const char VariablePrefix = '$';
		const string identifierPattern = @"[a-zA-Z][a-zA-Z0-9]*";
		public static readonly string VariablePattern = $@"\{VariablePrefix}{identifierPattern}(?:\s*\.\s*{identifierPattern})*";

		const string DefaultBankName = "Default";
		const char BankSeparator = '.';

		readonly Dictionary<string, ScriptVariableBank> variableBanks = new(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, ScriptVariableBank> VariableBanks => variableBanks;

		void Start()
		{
			// Initialize default variables
			foreach (var (variableName, variableValue) in DefaultVariables.Variables)
			{
				Set(variableName, variableValue);
			}
		}

		public object Get(string name)
		{
			(string bankName, string variableName) = ParseVariableName(name);

			if (!variableBanks.TryGetValue(bankName, out ScriptVariableBank variableBank)) return null;

			return variableBank.Get(variableName);
		}

		public void Set(string name, object value)
		{
			(string bankName, string variableName) = ParseVariableName(name);

			if (variableBanks.TryGetValue(bankName, out ScriptVariableBank variableBank))
			{
				variableBank.Set(variableName, value);
			}
			else
			{
				variableBank = new ScriptVariableBank();
				variableBank.Set(variableName, value);
				variableBanks.Add(bankName, variableBank);
			}

			UpdatePlayerProgress(name, value);
		}

		public void SetTyped(string name, string value, DataTypeEnum dataType)
		{
			if (dataType == DataTypeEnum.Int)
			{
				int.TryParse(value, out int intValue);
				Set(name, intValue);
			}
			else if (dataType == DataTypeEnum.Float)
			{
				float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue);
				Set(name, floatValue);
			}
			else if (dataType == DataTypeEnum.Bool)
			{
				bool.TryParse(value, out bool boolValue);
				Set(name, boolValue);
			}
			else
			{
				Set(name, value);
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

		void UpdatePlayerProgress(string variableName, object variableValue)
		{
			if (variableName.ToLower() != DefaultVariables.PlayerNameVariable.ToLower()) return;

			gameProgressManager.SetPlayerName(variableValue.ToString());
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
