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
	[SerializeField] float spriteTransitionSpeed;
	[SerializeField] float colorTransitionSpeed;
	[SerializeField][Range(0, 1)] float darkenBrightness;
	[SerializeField] bool spritesFacingRight;

	public Color DefaultTextColor { get { return defaultTextColor; } }
	public TMP_FontAsset DefaultFont { get { return defaultFont; } }
	public TextBuilder.TextMode TextMode { get { return textMode; } }
	public float TextSpeed { get { return textSpeed; } }
	public float CharacterShowSpeed { get { return characterShowSpeed; } }
	public float CharacterHideSpeed { get { return characterHideSpeed; } }
	public float SpriteTransitionSpeed { get { return spriteTransitionSpeed; } }
	public float ColorTransitionSpeed { get { return colorTransitionSpeed; } }
	public float DarkenBrightness { get { return darkenBrightness; } }
	public bool AreSpritesFacingRight { get { return spritesFacingRight; } }
}
