using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Audio
{
	public class AudioLayer
	{
		readonly int num;
		readonly AudioGroup audioGroup;
		readonly Dictionary<Guid, AudioTrack> tracks = new();
		readonly ObjectPool<AudioTrack> trackPool;

		public int Num => num;
		public Dictionary<Guid, AudioTrack> Tracks => tracks;

		public AudioLayer(AudioGroup audioGroup, int num)
		{
			this.audioGroup = audioGroup;
			this.num = num;
			trackPool = new ObjectPool<AudioTrack>(OnCreateTrack, OnGetTrack, OnReleaseTrack, OnDestroyTrack, maxSize: 1);
		}

		public IEnumerator Play(string audioName, float volume = 0.5f, float pitch = 1f, bool isLooping = false, bool isImmediate = false, float fadeSpeed = 0)
		{
			// Stop all tracks before starting the new one (sfx are excluded)
			if (audioGroup.Type != AudioType.SFX)
				Stop(null, isImmediate, fadeSpeed);

			AudioTrack track = trackPool.Get();
			track.Initialize(trackPool, audioGroup);

			Guid trackId = Guid.NewGuid();
			tracks[trackId] = track;
			yield return track.Play(trackId, audioName, volume, pitch, isLooping, isImmediate, fadeSpeed);
		}

		public void Stop(string audioName = null, bool isImmediate = false, float fadeSpeed = 0)
		{
			foreach (AudioTrack track in tracks.Values.ToList())
			{
				bool isValidAudioName = !string.IsNullOrEmpty(audioName);
				bool isSelectedTrack = isValidAudioName && track.Name == audioName;
				bool canBeCleared = !isValidAudioName || !track.IsActive || isSelectedTrack;
				if (!canBeCleared) continue;

				track.Stop(isImmediate, fadeSpeed);
			}
		}

		AudioTrack OnCreateTrack()
		{
			GameObject trackObject = new GameObject($"{audioGroup.Type} Track", typeof(RectTransform));
			AudioSource audioSource = trackObject.AddComponent<AudioSource>();
			AudioTrack audioTrack = trackObject.AddComponent<AudioTrack>();

			trackObject.transform.SetParent(audioGroup.Root, false);
			audioSource.outputAudioMixerGroup = audioGroup.MixerGroup;
			audioSource.pitch = 1f;
			audioSource.loop = false;
			audioSource.volume = 0f;

			return audioTrack;
		}

		void OnGetTrack(AudioTrack audioTrack)
		{
			audioTrack.gameObject.SetActive(true);
		}

		void OnReleaseTrack(AudioTrack audioTrack)
		{
			audioTrack.gameObject.SetActive(false);
			ClearTrack(audioTrack);
		}

		void OnDestroyTrack(AudioTrack audioTrack)
		{
			UnityEngine.Object.Destroy(audioTrack.gameObject);
		}

		void ClearTrack(AudioTrack track)
		{
			if (tracks.ContainsKey(track.Id))
				tracks.Remove(track.Id);
		}
	}
}
