using Dialogue;
using IO;
using TMPro;
using UnityEngine;
using Visuals;

namespace VN
{
	[CreateAssetMenu(fileName = "Game Options", menuName = "Scriptable Objects/Game Options")]
	public class VNOptionsSO : ScriptableObject
	{
		[SerializeField] GeneralOptions general;
		[SerializeField] DialogueOptions dialogue;
		[SerializeField] CharacterOptions characters;
		[SerializeField] Model3DOptions model3D;
		[SerializeField] ImageOptions images;
		[SerializeField] AudioOptions audio;
		[SerializeField] IOOptions inputOutput;

		public GeneralOptions General => general;
		public DialogueOptions Dialogue => dialogue;
		public CharacterOptions Characters => characters;
		public Model3DOptions Model3D => model3D;
		public ImageOptions Images => images;
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
			[SerializeField] ScreenMode screenMode;
			[SerializeField] GraphicsQuality graphicsQuality;
			[SerializeField] int resolutionWidth;
			[SerializeField] int resolutionHeight;

			public float TransitionSpeed => transitionSpeed;
			public float SkipTransitionSpeed => skipTransitionSpeed;
			public float SceneFadeTransitionSpeed => sceneFadeTransitionSpeed;

			public ScreenMode GameScreenMode => screenMode;
			public GraphicsQuality GraphicsQuality => graphicsQuality;
			public int ResolutionWidth => resolutionWidth;
			public int ResolutionHeight => resolutionHeight;
		}

		[System.Serializable]
		public class DialogueOptions
		{
			[Header("Text Speed")]
			[SerializeField] float textSpeed;
			[SerializeField] float minTextSpeed;
			[SerializeField] float maxTextSpeed;

			[Header("Auto Speed")]
			[SerializeField] float autoSpeed;
			[SerializeField] float minAutoSpeed;
			[SerializeField] float maxAutoSpeed;

			[Header("Skip Speed")]
			[SerializeField] float skipSpeed;

			[Header("Text Style")]
			[SerializeField] Color defaultTextColor;
			[SerializeField] TMP_FontAsset defaultFont;
			[SerializeField][Range(1, 100)] float dialogueFontSize;

			[Header("Reading Options")]
			[SerializeField] TextTypeMode textMode;
			[SerializeField] DialogueSkipMode skipMode;
			[SerializeField] PromptPosition promptPos;
			[SerializeField] bool stopAutoOnClick;

			public float TextSpeed => textSpeed;
			public float MinTextSpeed => minTextSpeed;
			public float MaxTextSpeed => maxTextSpeed;
			public float AutoSpeed => autoSpeed;
			public float MinAutoSpeed => minAutoSpeed;
			public float MaxAutoSpeed => maxAutoSpeed;
			public float SkipSpeed => skipSpeed;

			public Color DefaultTextColor => defaultTextColor;
			public TMP_FontAsset DefaultFont => defaultFont;
			public float DialogueFontSize => dialogueFontSize;

			public TextTypeMode TextMode => textMode;
			public DialogueSkipMode SkipMode => skipMode;
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
		public class ImageOptions
		{
			[SerializeField] int layers;
			[SerializeField] float transitionSpeed;

			public int Layers => layers;
			public float TransitionSpeed => transitionSpeed;
		}

		[System.Serializable]
		public class AudioOptions
		{
			[SerializeField] int layers;

			[Header("Transitions")]
			[SerializeField] float transitionSpeed;

			[Header("Mixer Volumes")]
			[SerializeField] float masterVolume;
			[SerializeField] float ambientVolume;
			[SerializeField] float musicVolume;
			[SerializeField] float sfxVolume;
			[SerializeField] float voiceVolume;

			public int Layers => layers;
			public float TransitionSpeed => transitionSpeed;
			public float MasterVolume => masterVolume;
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
			[SerializeField] FileExtension audioExtension;
			[SerializeField] FileExtension videoExtension;

			public bool UseSlotScreenshots => useSlotScreenshots;
			public bool HasAutosaveTimer => autosaveTimer;
			public float AutosaveTimerInterval => autosaveTimerInterval;
			public int SlotCount => slotCount;
			public FileExtension AudioExtension => audioExtension;
			public FileExtension VideoExtension => videoExtension;
		}
	}
}
