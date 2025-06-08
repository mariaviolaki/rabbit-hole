using Dialogue;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Options", menuName = "Scriptable Objects/Game Options")]
public class GameOptionsSO : ScriptableObject
{
	[SerializeField] GeneralOptions general;
	[SerializeField] IOOptions inputOutput;
	[SerializeField] DialogueOptions dialogue;
	[SerializeField] CharacterOptions characters;
	[SerializeField] Model3DOptions model3D;
	[SerializeField] BackgroundLayerOptions backgroundLayers;
	[SerializeField] AudioOptions audio;

	public GeneralOptions General { get { return general; } }
	public IOOptions IO { get { return inputOutput; } }
	public DialogueOptions Dialogue { get { return dialogue; } }
	public CharacterOptions Characters { get { return characters; } }
	public Model3DOptions Model3D { get { return model3D; } }
	public BackgroundLayerOptions BackgroundLayers { get { return backgroundLayers; } }
	public AudioOptions Audio { get { return audio; } }

	[System.Serializable]
	public class GeneralOptions
	{
		[SerializeField] float skipTransitionSpeed;

		public float SkipTransitionSpeed { get { return skipTransitionSpeed; } }
	}

	[System.Serializable]
	public class IOOptions
	{
		[SerializeField] GameIO.FileExtension audioExtension;
		[SerializeField] GameIO.FileExtension videoExtension;

		public GameIO.FileExtension AudioExtension { get { return audioExtension; } }
		public GameIO.FileExtension VideoExtension { get { return videoExtension; } }
	}

	[System.Serializable]
	public class DialogueOptions
	{
		[SerializeField] Color defaultTextColor;
		[SerializeField] TMP_FontAsset defaultFont;
		[SerializeField] TextBuilder.TextMode textMode;
		[SerializeField][Range(1, 12)] float textSpeed;
		[SerializeField] PromptPosition promptPos;
		[SerializeField][Range(1, 100)] float dialogueFontSize;
		[SerializeField] float fadeTransitionSpeed;
		[SerializeField] float sceneFadeTransitionSpeed;

		public Color DefaultTextColor { get { return defaultTextColor; } }
		public TMP_FontAsset DefaultFont { get { return defaultFont; } }
		public TextBuilder.TextMode TextMode { get { return textMode; } }
		public float TextSpeed { get { return textSpeed; } }
		public PromptPosition PromptPos { get { return promptPos; } }
		public float DialogueFontSize { get { return dialogueFontSize; } }
		public float FadeTransitionSpeed { get { return fadeTransitionSpeed; } }
		public float SceneFadeTransitionSpeed { get { return sceneFadeTransitionSpeed; } }
	}

	[System.Serializable]
	public class CharacterOptions
	{
		[SerializeField] float moveSpeed;
		[SerializeField] float fadeTransitionSpeed;
		[SerializeField] float brightnessTransitionSpeed;
		[SerializeField] float colorTransitionSpeed;
		[SerializeField][Range(0, 1)] float darkenBrightness;
		[SerializeField] bool spritesFacingRight;

		public float MoveSpeed { get { return moveSpeed; } }
		public float FadeTransitionSpeed { get { return fadeTransitionSpeed; } }
		public float BrightnessTransitionSpeed { get { return brightnessTransitionSpeed; } }
		public float ColorTransitionSpeed { get { return colorTransitionSpeed; } }
		public float DarkenBrightness { get { return darkenBrightness; } }
		public bool AreSpritesFacingRight { get { return spritesFacingRight; } }
	}

	[System.Serializable]
	public class Model3DOptions
	{
		[SerializeField] RenderTexture renderTexture3D;
		[SerializeField] float expressionTransitionSpeed;
		[SerializeField] float defaultAngle;

		public RenderTexture RenderTexture3D { get { return renderTexture3D; } }
		public float ExpressionTransitionSpeed { get { return expressionTransitionSpeed; } }
		public float DefaultAngle { get { return defaultAngle; } }
	}

	[System.Serializable]
	public class BackgroundLayerOptions
	{
		[SerializeField] float transitionSpeed;

		public float TransitionSpeed { get { return transitionSpeed; } }
	}

	[System.Serializable]
	public class AudioOptions
	{
		[SerializeField] float transitionSpeed;

		public float TransitionSpeed { get { return transitionSpeed; } }
	}
}
