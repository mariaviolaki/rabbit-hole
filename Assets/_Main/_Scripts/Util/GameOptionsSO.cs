using Dialogue;
using TMPro;
using UnityEngine;
using Visuals;

[CreateAssetMenu(fileName = "Game Options", menuName = "Scriptable Objects/Game Options")]
public class GameOptionsSO : ScriptableObject
{
	[SerializeField] GeneralOptions general;
	[SerializeField] DialogueOptions dialogue;
	[SerializeField] CharacterOptions characters;
	[SerializeField] Model3DOptions model3D;
	[SerializeField] BackgroundLayerOptions backgroundLayers;
	[SerializeField] AudioOptions audio;
	[SerializeField] IOOptions inputOutput;

	public GeneralOptions General => general;
	public DialogueOptions Dialogue => dialogue;
	public CharacterOptions Characters => characters;
	public Model3DOptions Model3D => model3D;
	public BackgroundLayerOptions BackgroundLayers => backgroundLayers;
	public AudioOptions Audio => audio;
	public IOOptions IO => inputOutput;

	[System.Serializable]
	public class GeneralOptions
	{
		[Header("Transitions")]
		[SerializeField] float transitionSpeed;
		[SerializeField] float skipTransitionSpeed;
		[SerializeField] float sceneFadeTransitionSpeed;

		[Header("Screen")]
		[SerializeField] bool isFullscreen;
		[SerializeField] GraphicsQuality graphicsQuality;
		[SerializeField] int resolutionWidth;
		[SerializeField] int resolutionHeight;

		public float TransitionSpeed => transitionSpeed;
		public float SkipTransitionSpeed => skipTransitionSpeed;
		public float SceneFadeTransitionSpeed => sceneFadeTransitionSpeed;

		public bool IsFullscreen => isFullscreen;
		public GraphicsQuality GraphicsQuality => graphicsQuality;
		public int ResolutionWidth => resolutionWidth;
		public int ResolutionHeight => resolutionHeight;
	}

	[System.Serializable]
	public class DialogueOptions
	{
		[Header("Text Speed")]
		[SerializeField][Range(1, 12)] float textSpeed;
		[SerializeField] float autoSpeed;
		[SerializeField] float skipSpeed;

		[Header("Text Style")]
		[SerializeField] Color defaultTextColor;
		[SerializeField] TMP_FontAsset defaultFont;
		[SerializeField][Range(1, 100)] float dialogueFontSize;

		[Header("Reading Options")]
		[SerializeField] TextBuildMode textMode;
		[SerializeField] DialogueSkipMode skipMode;
		[SerializeField] PromptPosition promptPos;
		[SerializeField] bool stopAutoOnClick;

		public float TextSpeed => textSpeed;
		public DialogueSkipMode SkipMode => skipMode;
		public float AutoSpeed => autoSpeed;
		public float SkipSpeed => skipSpeed;

		public Color DefaultTextColor => defaultTextColor;
		public TMP_FontAsset DefaultFont => defaultFont;
		public float DialogueFontSize => dialogueFontSize;

		public TextBuildMode TextMode => textMode;
		public PromptPosition PromptPos => promptPos;
		public bool StopAutoOnClick => stopAutoOnClick;
	}

	[System.Serializable]
	public class CharacterOptions
	{
		[Header("Transitions")]
		[SerializeField] float transitionSpeed;
		[SerializeField] float moveSpeed;

		[Header("Character Visuals")]
		[SerializeField][Range(0, 1)] float darkenBrightness;
		[SerializeField] bool spritesFacingRight;
		[SerializeField] bool highlightOnSpeak;

		public float TransitionSpeed => transitionSpeed;
		public float MoveSpeed => moveSpeed;
		public float DarkenBrightness => darkenBrightness;
		public bool AreSpritesFacingRight => spritesFacingRight;
		public bool HighlightOnSpeak => highlightOnSpeak;
	}

	[System.Serializable]
	public class Model3DOptions
	{
		[SerializeField] RenderTexture renderTexture3D;
		[SerializeField] float expressionTransitionSpeed;
		[SerializeField] float defaultAngle;

		public RenderTexture RenderTexture3D => renderTexture3D;
		public float DefaultAngle => defaultAngle;
	}

	[System.Serializable]
	public class BackgroundLayerOptions
	{
		[SerializeField] float transitionSpeed;

		public float TransitionSpeed => transitionSpeed;
	}

	[System.Serializable]
	public class AudioOptions
	{
		[Header("Transitions")]
		[SerializeField] float transitionSpeed;

		[Header("Mixer Volumes")]
		[SerializeField] float defaultVolume;
		[SerializeField] float ambientVolume;
		[SerializeField] float musicVolume;
		[SerializeField] float sfxVolume;
		[SerializeField] float voiceVolume;

		public float TransitionSpeed => transitionSpeed;
		public float DefaultVolume => defaultVolume;
		public float AmbientVolume => ambientVolume;
		public float MusicVolume => musicVolume;
		public float SFXVolume => sfxVolume;
		public float VoiceVolume => voiceVolume;
	}

	[System.Serializable]
	public class IOOptions
	{
		[Header("Save Files")]
		[SerializeField] bool useSlotScreenshots;
		[SerializeField] bool autosaveTimer;
		[SerializeField][Range(30, 600)] float autosaveTimerInterval;
		[SerializeField] int slotCount;

		[Header("Default Extensions")]
		[SerializeField] IO.FileExtension audioExtension;
		[SerializeField] IO.FileExtension videoExtension;

		public bool UseSlotScreenshots => useSlotScreenshots;
		public bool HasAutosaveTimer => autosaveTimer;
		public float AutosaveTimerInterval => autosaveTimerInterval;
		public int SlotCount => slotCount;
		public IO.FileExtension AudioExtension => audioExtension;
		public IO.FileExtension VideoExtension => videoExtension;
	}
}
