using System;
using System.Collections.Generic;
using UnityEngine;

namespace Variables
{
	public class ScriptVariableBank
	{
		readonly Dictionary<string, ScriptVariable> variables = new(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, ScriptVariable> Variables => variables;

		public object Get(string name)
		{
			if (!variables.TryGetValue(name, out ScriptVariable variable)) return null;

			return variable.Get();
		}

		public void Set(string name, object value)
		{
			if (variables.TryGetValue(name, out ScriptVariable variable))
			{
				Type variableType = variable.Get().GetType();
				Type newVariableType = value.GetType();

				if (variableType == newVariableType)
				{
					variable.Set(value);
				}
				else if (CanOverwriteVariable(variableType, newVariableType))
				{
					// In certain cases overwrite the type of the initial variable
					Remove(name);
					variables.Add(name, new ScriptVariable(value));
				}
				else
				{
					Debug.LogWarning($"Unable to assign value to Script Variable '{name}' because it is not of type {variableType.Name}.");
				}
			}
			else
			{
				variables.Add(name, new ScriptVariable(value));
			}
		}

		public void Remove(string name)
		{
			variables.Remove(name);
		}

		public void RemoveAll()
		{
			variables.Clear();
		}

		bool CanOverwriteVariable(Type existingType, Type newType)
		{
			if (newType == typeof(string) || existingType == typeof(string)) return true;
			if (existingType == typeof(int) && newType == typeof(float)) return true;
			return false;
		}
	}
}
