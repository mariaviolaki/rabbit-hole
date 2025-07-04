using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Variables;

public static class Utilities
{
	public static Color GetColorFromHex(string hexColor)
	{
		return ColorUtility.TryParseHtmlString(hexColor, out Color color) ? color : Color.white;
	}

	public static bool AreApproximatelyEqual(float a, float b)
	{
		return Mathf.Approximately(a, b);
	}

	public static bool AreApproximatelyEqual(in Vector2 a, in Vector2 b)
	{
		return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
	}

	public static bool AreApproximatelyEqual(in Vector3 a, in Vector3 b)
	{
		return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
	}

	public static bool AreApproximatelyEqual(in Color a, in Color b)
	{
		return Mathf.Approximately(a.r, b.r) && Mathf.Approximately(a.g, b.g) &&
			Mathf.Approximately(a.b, b.b) && Mathf.Approximately(a.a, b.a);
	}

	public static DataTypeEnum GetDataTypeEnum(object value)
	{
		return value switch
		{
			bool => DataTypeEnum.Bool,
			int => DataTypeEnum.Int,
			float => DataTypeEnum.Float,
			_ => DataTypeEnum.String
		};
	}

	public static DataTypeEnum GetDataTypeEnum(AnimatorControllerParameterType type)
	{
		return type switch
		{
			AnimatorControllerParameterType.Bool => DataTypeEnum.Bool,
			AnimatorControllerParameterType.Int => DataTypeEnum.Int,
			AnimatorControllerParameterType.Float => DataTypeEnum.Float,
			_ => DataTypeEnum.None
		};
	}

	public static IEnumerator RunConcurrentProcesses(List<IEnumerator> processes)
	{
		bool[] runningProcesses = new bool[processes.Count];

		for (int i = 0; i < processes.Count; i++)
		{
			runningProcesses[i] = processes[i] != null;
		}

		while (true)
		{
			bool areProcessesRunning = false;
			for (int i = 0; i < processes.Count; i++)
			{

				if (!runningProcesses[i]) continue;

				runningProcesses[i] = processes[i].MoveNext();
				areProcessesRunning = true;
			}

			if (!areProcessesRunning) break;
			yield return null;
		}
	}
}
