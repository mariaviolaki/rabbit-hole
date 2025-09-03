using System.Collections.Generic;
using UnityEngine;
using Variables;

namespace History
{
	[System.Serializable]
	public class HistoryVariableData
	{
		[SerializeField] List<HistoryVariable> variables = new();

		public HistoryVariableData(VariableManager variableManager)
		{
			foreach (var variableBank in variableManager.VariableBanks)
			{
				foreach (var scriptVariable in variableBank.Value.Variables)
				{
					HistoryVariable historyVariable = new()
					{
						name = $"{variableBank.Key}.{scriptVariable.Key}",
						dataType = scriptVariable.Value.GetDataType(),
						value = scriptVariable.Value.Get()?.ToString() ?? ""
					};
					variables.Add(historyVariable);
				}
			}
		}

		public void Load(VariableManager variableManager)
		{
			variableManager.RemoveAllBanks();
			
			foreach (HistoryVariable historyVariable in variables)
			{
				variableManager.SetTyped(historyVariable.name, historyVariable.value, historyVariable.dataType, null, null);
			}
		}
	}
}
