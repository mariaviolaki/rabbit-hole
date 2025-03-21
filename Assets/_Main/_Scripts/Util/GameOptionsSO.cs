using Dialogue;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Options", menuName = "Scriptable Objects/Game Options")]
public class GameOptionsSO : ScriptableObject
{
	[Header("Dialogue")]
	[SerializeField] Color defaultTextColor;
	[SerializeField] TMP_FontAsset defaultFont;
	[SerializeField] TextBuilder.TextMode textMode;
	[SerializeField][Range(1, 12)] float textSpeed;

	[Header("Characters")]
	[SerializeField] float characterShowSpeed;
	[SerializeField] float characterHideSpeed;

	public Color DefaultTextColor { get { return defaultTextColor; } }
	public TMP_FontAsset DefaultFont { get { return defaultFont; } }
	public TextBuilder.TextMode TextMode { get { return textMode; } }
	public float TextSpeed { get { return textSpeed; } }
	public float CharacterShowSpeed { get { return characterShowSpeed; } }
	public float CharacterHideSpeed { get { return characterHideSpeed; } }
}
