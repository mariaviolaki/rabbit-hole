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
			masterVolumeSlider.SetValueWithoutNotify(gameStateManager.State.AudioVolume);
			ambientVolumeSlider.SetValueWithoutNotify(gameStateManager.State.AmbientAudioVolume);
			musicVolumeSlider.SetValueWithoutNotify(gameStateManager.State.MusicAudioVolume);
			sfxVolumeSlider.SetValueWithoutNotify(gameStateManager.State.SFXAudioVolume);
			voiceVolumeSlider.SetValueWithoutNotify(gameStateManager.State.VoiceAudioVolume);
		}

		void SetMasterVolume(float volume)
		{
			gameStateManager.State.SetVolume(volume);
		}

		void SetAmbientVolume(float volume)
		{
			gameStateManager.State.SetAmbientVolume(volume);
		}

		void SetMusicVolume(float volume)
		{
			gameStateManager.State.SetMusicVolume(volume);
		}

		void SetSFXVolume(float volume)
		{
			gameStateManager.State.SetSFXVolume(volume);
		}

		void SetVoiceVolume(float volume)
		{
			gameStateManager.State.SetVoiceVolume(volume);
		}

		public override void Reset()
		{
			gameStateManager.State.ResetVolume();
			gameStateManager.State.ResetAmbientVolume();
			gameStateManager.State.ResetMusicVolume();
			gameStateManager.State.ResetSFXVolume();
			gameStateManager.State.ResetVoiceVolume();
		}

		override protected void SubscribeListeners()
		{
			masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
			ambientVolumeSlider.onValueChanged.AddListener(SetAmbientVolume);
			musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
			sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
			voiceVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);
		}

		override protected void UnsubscribeListeners()
		{
			masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
			ambientVolumeSlider.onValueChanged.RemoveListener(SetAmbientVolume);
			musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
			sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
			voiceVolumeSlider.onValueChanged.RemoveListener(SetVoiceVolume);
		}
	}
}
