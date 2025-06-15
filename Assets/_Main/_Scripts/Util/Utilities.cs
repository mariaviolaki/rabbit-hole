using UnityEngine;

public static class Utilities
{
	public static Color GetColorFromHex(string hexColor)
	{
		return ColorUtility.TryParseHtmlString(hexColor, out Color color) ? color : Color.white;
	}
}
