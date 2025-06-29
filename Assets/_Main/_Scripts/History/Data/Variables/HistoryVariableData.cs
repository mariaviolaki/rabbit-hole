using System.Collections.Generic;
using UnityEngine;
using Variables;

namespace History
{
	[System.Serializable]
	public class HistoryVariableData
	{
		[SerializeField] List<HistoryVariable> variables = new();

		public HistoryVariableData(ScriptVariableManager variableManager, ScriptTagManager tagManager)
		{
			foreach (var variableBank in variableManager.VariableBanks)
			{
				foreach (var scriptVariable in variableBank.Value.Variables)
				{
					HistoryVariable historyVariable = new()
					{
						type = ScriptVariableType.Variable,
						name = $"{variableBank.Key}.{scriptVariable.Key}",
						dataType = scriptVariable.Value.GetDataType(),
						value = scriptVariable.Value.Get()?.ToString() ?? ""
					};
					variables.Add(historyVariable);
				}
			}

			foreach (DialogueTag tag in tagManager.TagBank.Tags.Values)
			{
				HistoryVariable historyVariable = new()
				{
					type = ScriptVariableType.Tag,
					name = tag.Name,
					dataType = DataTypeEnum.String,
					value = tag.CurrentValue
				};
				variables.Add(historyVariable);
			}
		}

		public void Apply(ScriptVariableManager variableManager, ScriptTagManager tagManager)
		{
			variableManager.RemoveAllBanks();
			
			foreach (HistoryVariable historyVariable in variables)
			{
				if (historyVariable.type == ScriptVariableType.Variable)
					variableManager.SetTyped(historyVariable.name, historyVariable.value, historyVariable.dataType, null, null);
				else if (historyVariable.type == ScriptVariableType.Tag)
					tagManager.SetTagValue(historyVariable.name, historyVariable.value);
			}
		}
	}
}
