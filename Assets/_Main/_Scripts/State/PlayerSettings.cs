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
		public PlayerSettings(VNOptionsSO vnOptions)
		{
			audioVolume = vnOptions.Audio.MasterVolume;
			ambientAudioVolume = vnOptions.Audio.AmbientVolume;
			musicAudioVolume = vnOptions.Audio.MusicVolume;
			sfxAudioVolume = vnOptions.Audio.SFXVolume;
			voiceAudioVolume = vnOptions.Audio.VoiceVolume;

			skipMode = vnOptions.Dialogue.SkipMode;
			textSpeed = vnOptions.Dialogue.TextSpeed;
			autoSpeed = vnOptions.Dialogue.AutoSpeed;

			screenMode = vnOptions.General.GameScreenMode;
			graphicsQuality = (int)vnOptions.General.GraphicsQuality;
			resolutionWidth = vnOptions.General.ResolutionWidth;
			resolutionHeight = vnOptions.General.ResolutionHeight;
		}
	}
}
