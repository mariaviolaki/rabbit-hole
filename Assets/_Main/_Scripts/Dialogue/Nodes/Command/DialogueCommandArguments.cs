using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Dialogue
{
	public class DialogueCommandArguments
	{
		const char ArgumentValueSeparator = '=';

		readonly Dictionary<string, string> namedArguments = new(StringComparer.OrdinalIgnoreCase);
		readonly List<string> indexedArguments = new();

		public List<string> IndexedArguments => indexedArguments;

		public DialogueCommandArguments()
		{
			InitArguments(new List<string>());
		}

		public DialogueCommandArguments(List<string> parsedArguments)
		{
			InitArguments(parsedArguments);
		}

		public bool Has<T>(int index, string key)
		{
			// Search if the argument was provided in the command with its key
			if (!namedArguments.TryGetValue(key, out string stringValue))
			{
				// If not, check if it was provided in the correct order without a key
				if (index < indexedArguments.Count)
					stringValue = indexedArguments[index];
				else
					return false; // this argument was not provided or is invalid
			}

			return IsArgumentOfType<T>(stringValue);
		}

		public T Get<T>(int index, string key, T defaultValue = default)
		{
			// Search if the argument was provided in the command with its key
			if (!namedArguments.TryGetValue(key, out string stringValue))
			{
				// If not, check if it was provided in the correct order without a key
				if (index < indexedArguments.Count)
					stringValue = indexedArguments[index];
				else
					return defaultValue; // this argument was not provided or is invalid
			}

			return ParseArgument(stringValue, defaultValue);
		}

		public void AddIndexedArgument(int index, string argument)
		{
			index = Mathf.Clamp(index, 0, indexedArguments.Count);
			indexedArguments.Insert(index, argument?.Trim());
		}

		public void AddNamedArgument(string key, string argument)
		{
			if (string.IsNullOrWhiteSpace(key)) return;
			namedArguments[key.Trim()] = argument?.Trim();
		}

		bool IsArgumentOfType<T>(string arg)
		{
			if (typeof(T) == typeof(bool) && bool.TryParse(arg, out bool parsedBool))
				return true;
			else if (typeof(T) == typeof(int) && int.TryParse(arg, out int parsedInt))
				return true;
			else if (typeof(T) == typeof(float) && float.TryParse(arg, out float parsedFloat))
				return true;
			else if (typeof(T).IsEnum && Enum.TryParse(typeof(T), arg, ignoreCase: true, out object parsedEnum))
				return true;
			else if (typeof(T) == typeof(string))
				return true;

			return false;
		}

		T ParseArgument<T>(string arg, T defaultValue = default)
		{
			T resultValue = defaultValue;

			if (typeof(T) == typeof(bool) && bool.TryParse(arg, out bool parsedBool))
				resultValue = (T)(object)parsedBool;
			else if (typeof(T) == typeof(int) && int.TryParse(arg, out int parsedInt))
				resultValue = (T)(object)parsedInt;
			else if (typeof(T) == typeof(float) && float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat))
				resultValue = (T)(object)parsedFloat;
			else if (typeof(T).IsEnum && Enum.TryParse(typeof(T), arg, ignoreCase: true, out object parsedEnum))
				resultValue = (T)parsedEnum;
			else if (typeof(T) == typeof(string))
				resultValue = (T)(object)arg;

			return resultValue;
		}

		void InitArguments(List<string> parsedArguments)
		{
			foreach (string argument in parsedArguments)
			{
				int separatorIndex = argument.IndexOf(ArgumentValueSeparator);

				if (separatorIndex == -1)
				{
					// Arguments without keys must always come before any named ones
					if (namedArguments.Count > 0)
						break;

					indexedArguments.Add(argument.Trim());
				}
				else
				{
					string argumentName = argument.Substring(0, separatorIndex).Trim();
					string argumentValue = argument.Substring(separatorIndex + 1).Trim();

					if (!namedArguments.ContainsKey(argumentName))
						namedArguments.Add(argumentName, argumentValue);
				}
			}
		}
	}
}
