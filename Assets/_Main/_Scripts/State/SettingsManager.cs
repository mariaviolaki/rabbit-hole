using Audio;
using Dialogue;
using History;
using IO;
using UnityEngine;
using VN;

namespace Game
{
	public class SettingsManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] SaveFileManagerSO saveFileManager;
		[SerializeField] AudioManager audioManager;
		[SerializeField] GameManager gameManager;

		PlayerSettings settings;
		bool hasPendingSettings = false;

		public float AudioVolume => settings.audioVolume;
		public float AmbientAudioVolume => settings.ambientAudioVolume;
		public float MusicAudioVolume => settings.musicAudioVolume;
		public float SFXAudioVolume => settings.sfxAudioVolume;
		public float VoiceAudioVolume => settings.voiceAudioVolume;
		public DialogueSkipMode SkipMode => settings.skipMode;
		public float TextSpeed => settings.textSpeed;
		public float AutoSpeed => settings.autoSpeed;
		public ScreenMode GameScreenMode => settings.screenMode;
		public int GraphicsQuality => settings.graphicsQuality;
		public int ResolutionWidth => settings.resolutionWidth;
		public int ResolutionHeight => settings.resolutionHeight;

		public void ResetVolume() => SetVolume(vnOptions.Audio.MasterVolume);
		public void ResetAmbientVolume() => SetAmbientVolume(vnOptions.Audio.AmbientVolume);
		public void ResetMusicVolume() => SetMusicVolume(vnOptions.Audio.MusicVolume);
		public void ResetSFXVolume() => SetSFXVolume(vnOptions.Audio.SFXVolume);
		public void ResetVoiceVolume() => SetVoiceVolume(vnOptions.Audio.VoiceVolume);
		public void ResetSkipMode() => SetSkipMode(vnOptions.Dialogue.SkipMode);
		public void ResetTextSpeed() => SetTextSpeed(vnOptions.Dialogue.TextSpeed);
		public void ResetAutoSpeed() => SetAutoSpeed(vnOptions.Dialogue.AutoSpeed);
		public void ResetScreenMode() => SetScreenMode(vnOptions.General.GameScreenMode);
		public void ResetGraphicsQuality() => SetGraphicsQuality((int)vnOptions.General.GraphicsQuality);
		public void ResetResolution() => SetResolution(vnOptions.General.ResolutionWidth, vnOptions.General.ResolutionHeight);

		void Awake()
		{
			LoadPlayerSettings();
		}

		void OnApplicationQuit()
		{
			SavePlayerSettings();
		}

		public bool SavePlayerSettings()
		{
			if (!hasPendingSettings) return true;

			if (saveFileManager.SavePlayerSettings(settings))
			{
				hasPendingSettings = false;
				return true;
			}

			return false;
		}

		public void SetVolume(float volume)
		{
			audioManager.SetVolume(Audio.AudioType.None, volume);
			settings.audioVolume = volume;
			hasPendingSettings = true;
		}

		public void SetAmbientVolume(float volume)
		{
			audioManager.SetVolume(Audio.AudioType.Ambient, volume);
			settings.ambientAudioVolume = volume;
			hasPendingSettings = true;
		}

		public void SetMusicVolume(float volume)
		{
			audioManager.SetVolume(Audio.AudioType.Music, volume);
			settings.musicAudioVolume = volume;
			hasPendingSettings = true;
		}

		public void SetSFXVolume(float volume)
		{
			audioManager.SetVolume(Audio.AudioType.SFX, volume);
			settings.sfxAudioVolume = volume;
			hasPendingSettings = true;
		}

		public void SetVoiceVolume(float volume)
		{
			audioManager.SetVolume(Audio.AudioType.Voice, volume);
			settings.voiceAudioVolume = volume;
			hasPendingSettings = true;
		}

		public void SetSkipMode(DialogueSkipMode skipMode)
		{
			settings.skipMode = skipMode;
			hasPendingSettings = true;
		}

		public void SetTextSpeed(float speed)
		{
			settings.textSpeed = speed;
			hasPendingSettings = true;
		}

		public void SetAutoSpeed(float speed)
		{
			settings.autoSpeed = speed;
			hasPendingSettings = true;
		}

		public void SetScreenMode(ScreenMode screenMode)
		{
			Screen.fullScreen = screenMode == ScreenMode.Fullscreen;
			settings.screenMode = screenMode;
			hasPendingSettings = true;
		}

		public void SetGraphicsQuality(int qualityIndex)
		{
			QualitySettings.SetQualityLevel(qualityIndex);
			settings.graphicsQuality = qualityIndex;
			hasPendingSettings = true;
		}

		public void SetResolution(int width, int height)
		{
			Screen.SetResolution(width, height, settings.screenMode == ScreenMode.Fullscreen);
			settings.resolutionWidth = width;
			settings.resolutionHeight = height;
			hasPendingSettings = true;
		}

		void LoadPlayerSettings()
		{
			if (saveFileManager.HasSettingsSave())
			{
				settings = saveFileManager.LoadPlayerSettings();
				if (settings != null) return;
			}

			settings = new(vnOptions);
			saveFileManager.SavePlayerSettings(settings);
		}
	}
}
