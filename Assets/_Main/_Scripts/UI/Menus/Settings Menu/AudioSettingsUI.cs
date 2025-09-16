using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class AudioSettingsUI : SettingsSectionBaseUI
	{
		[SerializeField] Slider masterVolumeSlider;
		[SerializeField] Slider ambientVolumeSlider;
		[SerializeField] Slider musicVolumeSlider;
		[SerializeField] Slider sfxVolumeSlider;
		[SerializeField] Slider voiceVolumeSlider;

		void Start()
		{
			masterVolumeSlider.SetValueWithoutNotify(settingsManager.AudioVolume);
			ambientVolumeSlider.SetValueWithoutNotify(settingsManager.AmbientAudioVolume);
			musicVolumeSlider.SetValueWithoutNotify(settingsManager.MusicAudioVolume);
			sfxVolumeSlider.SetValueWithoutNotify(settingsManager.SFXAudioVolume);
			voiceVolumeSlider.SetValueWithoutNotify(settingsManager.VoiceAudioVolume);
		}

		public override void Reset()
		{
			settingsManager.ResetVolume();
			settingsManager.ResetAmbientVolume();
			settingsManager.ResetMusicVolume();
			settingsManager.ResetSFXVolume();
			settingsManager.ResetVoiceVolume();

			masterVolumeSlider.SetValueWithoutNotify(settingsManager.AudioVolume);
			ambientVolumeSlider.SetValueWithoutNotify(settingsManager.AmbientAudioVolume);
			musicVolumeSlider.SetValueWithoutNotify(settingsManager.MusicAudioVolume);
			sfxVolumeSlider.SetValueWithoutNotify(settingsManager.SFXAudioVolume);
			voiceVolumeSlider.SetValueWithoutNotify(settingsManager.VoiceAudioVolume);
		}

		void SetMasterVolume(float volume)
		{
			settingsManager.SetVolume(volume);
		}

		void SetAmbientVolume(float volume)
		{
			settingsManager.SetAmbientVolume(volume);
		}

		void SetMusicVolume(float volume)
		{
			settingsManager.SetMusicVolume(volume);
		}

		void SetSFXVolume(float volume)
		{
			settingsManager.SetSFXVolume(volume);
		}

		void SetVoiceVolume(float volume)
		{
			settingsManager.SetVoiceVolume(volume);
		}

		override protected void SubscribeListeners()
		{
			resetButton.onClick.AddListener(Reset);
			masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
			ambientVolumeSlider.onValueChanged.AddListener(SetAmbientVolume);
			musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
			sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
			voiceVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);
		}

		override protected void UnsubscribeListeners()
		{
			resetButton.onClick.RemoveListener(Reset);
			masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
			ambientVolumeSlider.onValueChanged.RemoveListener(SetAmbientVolume);
			musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
			sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
			voiceVolumeSlider.onValueChanged.RemoveListener(SetVoiceVolume);
		}
	}
}
