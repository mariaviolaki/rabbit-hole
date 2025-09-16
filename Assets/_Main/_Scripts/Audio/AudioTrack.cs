using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using VN;

namespace Audio
{
	public class AudioTrack : MonoBehaviour
	{
		enum TrackStatus { Started, Stopped }

		const float FadeSpeedMultiplier = 20f;

		VNOptionsSO vnOptions;
		ObjectPool<AudioTrack> trackPool;
		AudioGroup audioGroup;
		AudioSource audioSource;

		Guid id;
		TrackStatus status;
		string trackName;
		float pitch;
		bool isLooping;
		float targetVolume;
		float fadeSpeed;
		float stopTime;

		public Guid Id => id;
		public string Name => trackName;
		public float Volume => targetVolume;
		public float Pitch => pitch;
		public bool IsLooping => isLooping;
		public bool IsActive => audioSource.clip != null;

		void Update()
		{
			FadeInVolume();
			StopAfterDuration();
		}

		public void Initialize(ObjectPool<AudioTrack> trackPool, AudioGroup audioGroup)
		{
			this.audioSource = GetComponent<AudioSource>();
			this.vnOptions = audioGroup.Manager.Options;
			this.trackPool = trackPool;
			this.audioGroup = audioGroup;
			status = TrackStatus.Stopped;
			stopTime = 0f;
		}

		public IEnumerator Play(Guid id, string trackName, float volume, float pitch, bool isLooping, bool isImmediate = false, float fadeSpeed = 0f)
		{
			if (status == TrackStatus.Started) yield break;

			status = TrackStatus.Started;
			yield return LoadAudioTrack(trackName);

			if (audioSource.clip == null)
			{
				UnloadAudioTrack();
				yield break;
			}

			this.id = id;
			this.trackName = trackName;
			this.pitch = pitch;
			this.isLooping = isLooping;
			this.targetVolume = volume;
			SetTransitionSpeed(fadeSpeed);

			audioSource.pitch = pitch;
			audioSource.loop = isLooping;
			audioSource.volume = isImmediate ? volume : 0f;

			audioSource.Play();

			if (isLooping)
			{
				stopTime = 0f;
			}
			else
			{
				float duration = audioSource.clip.length / pitch;
				stopTime = Time.time + duration + 1;
			}
		}

		public void Stop(bool isImmediate = false, float fadeSpeed = 0f)
		{
			targetVolume = 0f;

			if (isImmediate)
			{
				StopImmediate();
			}
			else
			{
				SetTransitionSpeed(fadeSpeed);
				stopTime = Time.time;
			}
		}

		void StopImmediate()
		{
			if (audioSource.clip == null) return;

			audioSource.Stop();
			UnloadAudioTrack();
		}

		void FadeInVolume()
		{
			if (audioSource.clip == null || status != TrackStatus.Started || Utilities.AreApproximatelyEqual(audioSource.volume, targetVolume)) return;

			float speed = fadeSpeed * Time.deltaTime;
			audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, speed);

			if (Utilities.AreApproximatelyEqual(audioSource.volume, targetVolume))
			{
				audioSource.volume = targetVolume;

				if (Utilities.AreApproximatelyEqual(targetVolume, 0f))
					StopImmediate();
			}
		}

		void StopAfterDuration()
		{
			if (audioSource.clip == null || isLooping || stopTime < Mathf.Epsilon || Time.time < stopTime || status == TrackStatus.Stopped) return;

			StopImmediate();
		}

		IEnumerator LoadAudioTrack(string name)
		{
			yield return audioGroup.Manager.Assets.LoadAudio(name, audioGroup.AssetLabel);
			audioSource.clip = audioGroup.Manager.Assets.GetAudio(name, audioGroup.AssetLabel);
		}

		void UnloadAudioTrack()
		{
			if (audioSource.clip == null) return;

			audioGroup.Manager.Assets.UnloadAudio(trackName, audioGroup.AssetLabel);
			audioSource.clip = null;
			status = TrackStatus.Stopped;
			RemoveFromPool();
		}

		void RemoveFromPool()
		{
			trackPool.Release(this);
		}

		void SetTransitionSpeed(float speed)
		{
			float baseSpeed = speed < Mathf.Epsilon ? vnOptions.Audio.TransitionSpeed : speed;
			this.fadeSpeed = baseSpeed * FadeSpeedMultiplier;
		}
	}
}
