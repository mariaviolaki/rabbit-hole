using System;
using System.Collections.Generic;
using System.Linq;

namespace Audio
{
	public class AudioLayer
	{
		readonly int num;
		readonly AudioGroup audioGroup;
		readonly Dictionary<Guid, AudioTrack> tracks = new();

		public int Num => num;

		public AudioLayer(AudioGroup audioGroup, int num)
		{
			this.audioGroup = audioGroup;
			this.num = num;
		}

		public void Play(string audioName, float volume = 0.5f, float pitch = 1f, bool isLooping = false, bool isImmediate = false, float fadeSpeed = 0)
		{
			PlayTrack(audioName, volume, pitch, isLooping, isImmediate, fadeSpeed);
		}

		public void Stop(string audioName = null, bool isImmediate = false, float fadeSpeed = 0)
		{
			StopTracks(audioName, isImmediate, fadeSpeed);
		}

		void PlayTrack(string audioName, float volume, float pitch, bool isLooping, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (audioGroup.Type != AudioType.SFX)
				StopTracks(null, isImmediate, fadeSpeed);

			Guid trackId = Guid.NewGuid();
			AudioTrack track = new AudioTrack(audioGroup, trackId, audioName);
			track.OnUnloadTrack += ClearTrack;
			tracks[trackId] = track;

			track.Play(volume, pitch, isLooping, isImmediate, fadeSpeed);
		}

		void ClearTrack(Guid id)
		{
			if (tracks.ContainsKey(id))
				tracks.Remove(id);
		}

		void StopTracks(string audioName, bool isImmediate = false, float fadeSpeed = 0)
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
	}
}
