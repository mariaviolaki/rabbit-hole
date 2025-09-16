using Game;
using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace Audio
{
	public class AudioManager : MonoBehaviour
	{
		const float MinVolume = 0.0001f;
		const float MaxVolume = 1f;

		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] AudioMixer audioMixer;
		[SerializeField] AudioMixerGroup ambientMixerGroup;
		[SerializeField] AudioMixerGroup musicMixerGroup;
		[SerializeField] AudioMixerGroup sfxMixerGroup;
		[SerializeField] AudioMixerGroup voiceMixerGroup;
		[SerializeField] AssetLabelReference ambientAddressableLabel;
		[SerializeField] AssetLabelReference musicAddressableLabel;
		[SerializeField] AssetLabelReference sfxAddressableLabel;
		[SerializeField] AssetLabelReference voiceAddressableLabel;
		[SerializeField] GameSceneManager sceneManager;

		public AssetManagerSO Assets => assetManager;
		public VNOptionsSO Options => vnOptions;
		public AudioMixer Mixer => audioMixer;
		public AudioMixerGroup AmbientMixerGroup => ambientMixerGroup;
		public AudioMixerGroup MusicMixerGroup => musicMixerGroup;
		public AudioMixerGroup SFXMixerGroup => sfxMixerGroup;
		public AudioMixerGroup VoiceMixerGroup => voiceMixerGroup;

		readonly Dictionary<AudioType, AudioGroup> audioGroups = new();

		public Dictionary<AudioType, AudioGroup> AudioGroups => audioGroups;

		void Awake()
		{
			audioGroups[AudioType.Ambient] = new AudioGroup(this, AudioType.Ambient, ambientMixerGroup, ambientAddressableLabel);
			audioGroups[AudioType.Music] = new AudioGroup(this, AudioType.Music, musicMixerGroup, musicAddressableLabel);
			audioGroups[AudioType.SFX] = new AudioGroup(this, AudioType.SFX, sfxMixerGroup, sfxAddressableLabel);
			audioGroups[AudioType.Voice] = new AudioGroup(this, AudioType.Voice, voiceMixerGroup, voiceAddressableLabel);

			Create(AudioType.Ambient, vnOptions.Audio.Layers);
			Create(AudioType.Music, vnOptions.Audio.Layers);
			Create(AudioType.SFX, vnOptions.Audio.Layers);
			Create(AudioType.Voice, vnOptions.Audio.Layers);
		}

		void OnEnable()
		{
			sceneManager.OnLoadSceneStart += StopAudioOnSceneChange;
		}

		void OnDisable()
		{
			sceneManager.OnLoadSceneStart -= StopAudioOnSceneChange;
		}

		public IEnumerator Play(AudioType audioType, string audioName, float volume = 0.5f, float pitch = 1f, bool isLooping = false, bool isImmediate = false, float fadeSpeed = 0f, int layerNum = 0)
		{
			yield return audioGroups[audioType].Play(audioName, volume, pitch, isLooping, isImmediate, fadeSpeed, layerNum);
		}

		public void Stop(AudioType audioType, string audioName, bool isImmediate = false, float fadeSpeed = 0f, int layerNum = 0)
		{
			audioGroups[audioType].Stop(audioName, isImmediate, fadeSpeed, layerNum);
		}

		public void StopAll(bool isImmediate = false, float fadeSpeed = 0f)
		{
			foreach (AudioGroup audioGroup in audioGroups.Values)
			{
				audioGroup.StopAll(isImmediate, fadeSpeed);
			}
		}

		public float GetVolume(AudioType audioType)
		{
			string volumeParam = audioType == AudioType.None ? "MasterVolume" : $"{audioType}Volume";
			float normalizedVolume = 0f;

			if (audioMixer.GetFloat(volumeParam, out float decibelVolume))
				normalizedVolume = Mathf.Pow(10f, decibelVolume / 20f);

			return normalizedVolume;
		}

		public void SetVolume(AudioType audioType, float normalizedVolume)
		{
			string volumeParam = audioType == AudioType.None ? "MasterVolume" : $"{audioType}Volume";
			float decibelVolume = Mathf.Log10(Mathf.Clamp(normalizedVolume, MinVolume, MaxVolume)) * 20f;

			audioMixer.SetFloat(volumeParam, decibelVolume);
		}

		void Create(AudioType audioType, int layerCount = 1)
		{
			audioGroups[audioType].CreateLayers(layerCount);
		}

		void StopAudioOnSceneChange()
		{
			StopAll(true);
		}
	}
}
