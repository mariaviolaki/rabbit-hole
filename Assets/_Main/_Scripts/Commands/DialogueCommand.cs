using System;
using UnityEngine;

namespace Commands
{
	public abstract class DialogueCommand
	{
		// Classes inheriting from DialogueCommand must redefine this function
		public static void Register(CommandDirectory commandDirectory) { }

		protected static T ParseArgument<T>(string arg, T defaultValue = default)
		{
			T resultValue = defaultValue;

			if (typeof(T) == typeof(bool) && bool.TryParse(arg, out bool parsedBool))
				resultValue = (T)(object)parsedBool;
			else if (typeof(T) == typeof(int) && int.TryParse(arg, out int parsedInt))
				resultValue = (T)(object)parsedInt;
			else if (typeof(T) == typeof(float) && float.TryParse(arg, out float parsedFloat))
				resultValue = (T)(object)parsedFloat;
			else if (typeof(T).IsEnum && Enum.TryParse(typeof(T), arg, ignoreCase: true, out object parsedEnum))
				resultValue = (T)parsedEnum;
			else if (typeof(T) == typeof(string))
				resultValue = (T)(object)arg;

			return resultValue;
		}

		protected static Color GetColorFromHex(string hexColor)
		{
			return ColorUtility.TryParseHtmlString(hexColor, out Color color) ? color : Color.white;
		}
	}
}
