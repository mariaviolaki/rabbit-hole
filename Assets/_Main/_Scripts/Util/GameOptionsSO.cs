using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Options", menuName = "Scriptable Objects/Game Options")]
public class GameOptionsSO : ScriptableObject
{
	[SerializeField] Color defaultTextColor;
	[SerializeField] TMP_FontAsset defaultFont;

	public Color DefaultTextColor { get { return defaultTextColor; } }
	public TMP_FontAsset DefaultFont { get { return defaultFont; } }
}
