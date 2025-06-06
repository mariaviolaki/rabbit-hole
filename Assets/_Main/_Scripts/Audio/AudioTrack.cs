using System;
using System.Collections;
using UnityEngine;

namespace Audio
{
	public class AudioTrack
	{
		const float FadeSpeedMultiplier = 20f;

		readonly Guid id;
		readonly string name;
		readonly AudioGroup audioGroup;
		AudioSource audioSource;
		AudioClip audioClip;

		Coroutine playCoroutine;
		Coroutine stopCoroutine;
		Coroutine autoStopCoroutine;

		public event Action<Guid> OnUnloadTrack;

		public string Name => name;
		public bool IsActive => audioSource != null && audioSource.clip != null;

		public AudioTrack(AudioGroup audioGroup, Guid id, string name)
		{
			this.audioGroup = audioGroup;
			this.id = id;
			this.name = name;
		}

		public void PlayInstant(float volume, float pitch, bool isLooping)
		{
			if (playCoroutine != null) return;
			playCoroutine = audioGroup.Manager.StartCoroutine(LoadTrack(volume, pitch, isLooping, volume));
		}

		public void Play(float volume, float pitch, bool isLooping, float fadeSpeed)
		{
			if (playCoroutine != null) return;
			playCoroutine = audioGroup.Manager.StartCoroutine(PlayAndFadeIn(volume, pitch, isLooping, fadeSpeed));
		}

		public void StopInstant()
		{
			if (stopCoroutine != null) return;

			StopAudioCoroutine(ref playCoroutine);
			StopAudioCoroutine(ref autoStopCoroutine);

			UnloadTrack();
		}

		public void Stop(float fadeSpeed)
		{
			if (stopCoroutine != null) return;

			stopCoroutine = audioGroup.Manager.StartCoroutine(FadeOutAndStop(fadeSpeed));
		}

		IEnumerator LoadTrack(float volume, float pitch, bool isLooping, float startVolume)
		{
			yield return LoadAudioTrack();
			if (audioClip == null)
			{
				StopAudioCoroutine(ref stopCoroutine);
				StopAudioCoroutine(ref autoStopCoroutine);
				UnloadTrack();
				yield break;
			}

			audioSource = new GameObject(audioClip.name).AddComponent<AudioSource>();
			audioSource.transform.SetParent(audioGroup.Root);
			audioSource.outputAudioMixerGroup = audioGroup.MixerGroup;
			audioSource.clip = audioClip;
			audioSource.pitch = pitch;
			audioSource.loop = isLooping;
			audioSource.volume = startVolume;

			audioSource.Play();

			if (!isLooping)
				autoStopCoroutine = audioGroup.Manager.StartCoroutine(AutoStopTrack(audioClip.length, pitch));
		}

		void UnloadTrack()
		{
			if (audioSource != null)
			{
				UnityEngine.Object.Destroy(audioSource.gameObject);
				audioSource = null;
			}

			UnloadAudioTrack();
			audioClip = null;

			OnUnloadTrack?.Invoke(id);
		}

		IEnumerator AutoStopTrack(float clipLength, float pitch)
		{	
			float duration = clipLength / pitch;
			yield return new WaitForSeconds(duration + 1);

			StopAudioCoroutine(ref playCoroutine);
			StopAudioCoroutine(ref stopCoroutine);

			UnloadTrack();	
		}

		IEnumerator PlayAndFadeIn(float volume, float pitch, bool isLooping, float speed)
		{
			yield return LoadTrack(volume, pitch, isLooping, 0f);

			if (audioSource != null && audioSource.clip != null)
				yield return FadeVolume(volume, speed);

			playCoroutine = null;
		}

		IEnumerator FadeOutAndStop(float speed)
		{
			if (audioSource != null && audioSource.clip != null)
				yield return FadeVolume(0f, speed);

			StopAudioCoroutine(ref playCoroutine);
			StopAudioCoroutine(ref autoStopCoroutine);

			stopCoroutine = null;
			UnloadTrack();
		}

		IEnumerator FadeVolume(float targetVolume, float speed)
		{
			if (audioSource == null) yield break;

			speed = speed > Mathf.Epsilon ? speed : audioGroup.Manager.Options.Audio.TransitionSpeed;
			float startVolume = audioSource.volume;

			float duration = (1 / speed) * FadeSpeedMultiplier * Mathf.Abs(startVolume - targetVolume);
			float timeElapsed = 0;
			while (timeElapsed < duration)
			{
				timeElapsed += Time.deltaTime;
				audioSource.volume = Mathf.Lerp(startVolume, targetVolume, Mathf.Clamp01(timeElapsed / duration));
				yield return null;
			}

			audioSource.volume = targetVolume;
		}

		IEnumerator LoadAudioTrack()
		{
			switch (audioGroup.Type)
			{
				case AudioType.Ambient:
					yield return audioGroup.Manager.FileManager.LoadAmbientAudio(name);
					audioClip = audioGroup.Manager.FileManager.GetAmbientAudio(name);
					break;
				case AudioType.Music:
					yield return audioGroup.Manager.FileManager.LoadMusicAudio(name);
					audioClip = audioGroup.Manager.FileManager.GetMusicAudio(name);
					break;
				case AudioType.SFX:
					yield return audioGroup.Manager.FileManager.LoadSFXAudio(name);
					audioClip = audioGroup.Manager.FileManager.GetSFXAudio(name);
					break;
				case AudioType.Voice:
					yield return audioGroup.Manager.FileManager.LoadVoiceAudio(name);
					audioClip = audioGroup.Manager.FileManager.GetVoiceAudio(name);
					break;
			}
		}

		void UnloadAudioTrack()
		{
			switch (audioGroup.Type)
			{
				case AudioType.Ambient:
					audioGroup.Manager.FileManager.UnloadAmbientAudio(name);
					break;
				case AudioType.Music:
					audioGroup.Manager.FileManager.UnloadMusicAudio(name);
					break;
				case AudioType.SFX:
					audioGroup.Manager.FileManager.UnloadSFXAudio(name);
					break;
				case AudioType.Voice:
					audioGroup.Manager.FileManager.UnloadVoiceAudio(name);
					break;
			}
		}

		void StopAudioCoroutine(ref Coroutine coroutine)
		{
			if (coroutine == null) return;

			audioGroup.Manager.StopCoroutine(coroutine);
			coroutine = null;
		}
	}
}
