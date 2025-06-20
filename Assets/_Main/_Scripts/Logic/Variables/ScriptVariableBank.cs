using System;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
	public class ScriptVariableBank
	{
		readonly Dictionary<string, ScriptVariable> variables = new(StringComparer.OrdinalIgnoreCase);

		public object Get(string name)
		{
			if (!variables.TryGetValue(name, out ScriptVariable variable)) return null;

			return variable.Get();
		}

		public void Set<T>(string name, T value, Func<T> getter = null, Action<T> setter = null)
		{
			if (variables.TryGetValue(name, out ScriptVariable variable))
			{
				if (variable is ScriptVariable<T> typedVariable)
					typedVariable.Set(value);
				else
					Debug.LogWarning($"Unable to assign value to Script Variable '{name}' because it is not of type {typeof(T).Name}.");
			}
			else
			{
				variables.Add(name, new ScriptVariable<T>(value, getter, setter));
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
	}
}
