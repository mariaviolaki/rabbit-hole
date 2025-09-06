using Dialogue;
using IO;

namespace History
{
	[System.Serializable]
	public class PlayerSettings
	{
		public float audioVolume;
		public float ambientAudioVolume;
		public float musicAudioVolume;
		public float sfxAudioVolume;
		public float voiceAudioVolume;

		public DialogueSkipMode skipMode;
		public float textSpeed;
		public float autoSpeed;

		public ScreenMode screenMode;
		public int graphicsQuality;
		public int resolutionWidth;
		public int resolutionHeight;

		// Parameterless constructor needed for serialization
		public PlayerSettings() { }

		// Default constructor in case this class couldn't be loaded from a file
		public PlayerSettings(GameOptionsSO gameOptions)
		{
			audioVolume = gameOptions.Audio.MasterVolume;
			ambientAudioVolume = gameOptions.Audio.AmbientVolume;
			musicAudioVolume = gameOptions.Audio.MusicVolume;
			sfxAudioVolume = gameOptions.Audio.SFXVolume;
			voiceAudioVolume = gameOptions.Audio.VoiceVolume;

			skipMode = gameOptions.Dialogue.SkipMode;
			textSpeed = gameOptions.Dialogue.TextSpeed;
			autoSpeed = gameOptions.Dialogue.AutoSpeed;

			screenMode = gameOptions.General.GameScreenMode;
			graphicsQuality = (int)gameOptions.General.GraphicsQuality;
			resolutionWidth = gameOptions.General.ResolutionWidth;
			resolutionHeight = gameOptions.General.ResolutionHeight;
		}
	}
}
