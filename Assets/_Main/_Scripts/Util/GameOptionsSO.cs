using Dialogue;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Options", menuName = "Scriptable Objects/Game Options")]
public class GameOptionsSO : ScriptableObject
{
	[SerializeField] Color defaultTextColor;
	[SerializeField] TMP_FontAsset defaultFont;
	[SerializeField][Range(1, 12)] float textSpeed;
	[SerializeField] TextBuilder.TextMode textMode;

	public Color DefaultTextColor { get { return defaultTextColor; } }
	public TMP_FontAsset DefaultFont { get { return defaultFont; } }
	public TextBuilder.TextMode TextMode { get { return textMode; } }
	public float TextSpeed { get { return textSpeed; } }
}
