using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VN;

namespace History
{
	[System.Serializable]
	public class HistoryAudioData
	{
		[SerializeField] List<HistoryAudioTrack> audioTracks = new();

		public HistoryAudioData(AudioManager audioManager)
		{
			// Only cache the layers with active audio to save memory
			foreach (AudioGroup audioGroup in audioManager.AudioGroups.Values)
			{
				foreach (AudioLayer audioLayer in audioGroup.Layers.Values)
				{
					foreach (AudioTrack audioTrack in audioLayer.Tracks.Values)
					{
						HistoryAudioTrack historyAudioTrack = new();
						audioTracks.Add(historyAudioTrack);

						historyAudioTrack.audioType = audioGroup.Type;
						historyAudioTrack.layerNum = audioLayer.Num;
						historyAudioTrack.name = audioTrack.Name;
						historyAudioTrack.volume = audioTrack.Volume;
						historyAudioTrack.pitch = audioTrack.Pitch;
						historyAudioTrack.isLooping = audioTrack.IsLooping;
					}
				}
			}
		}

		public IEnumerator Load(AudioManager audioManager, VNOptionsSO vnOptions)
		{
			float fadeSpeed = vnOptions.General.SkipTransitionSpeed;

			// Mirror the stucture of audio tracks used in the manager for efficient lookups
			Dictionary<Audio.AudioType, Dictionary<int, List<HistoryAudioTrack>>> mappedAudio = GetMappedAudio();

			// Iterate through all the audio layers in the manager and update those which are different from the history state
			foreach (AudioGroup audioGroup in audioManager.AudioGroups.Values)
			{
				foreach (AudioLayer audioLayer in audioGroup.Layers.Values)
				{
					List<HistoryAudioTrack> historyAudioTracks = null;
					if (mappedAudio.TryGetValue(audioGroup.Type, out var audioLayers))
						audioLayers.TryGetValue(audioLayer.Num, out historyAudioTracks);

					historyAudioTracks ??= new();
					HashSet<string> audioTrackNames = new();

					foreach (AudioTrack audioTrack in audioLayer.Tracks.Values)
					{
						if (IsTrackInHistory(audioTrack, historyAudioTracks))
							audioTrackNames.Add(audioTrack.Name);
						else
							audioManager.Stop(audioGroup.Type, audioTrack.Name, false, fadeSpeed, audioLayer.Num);
					}

					foreach (HistoryAudioTrack historyAudioTrack in historyAudioTracks)
					{
						if (!audioTrackNames.Contains(historyAudioTrack.name))
						{
							yield return audioManager.Play(historyAudioTrack.audioType, historyAudioTrack.name, historyAudioTrack.volume,
								historyAudioTrack.pitch, historyAudioTrack.isLooping, false, fadeSpeed, historyAudioTrack.layerNum);
						}
					}
				}
			}
		}

		Dictionary<Audio.AudioType, Dictionary<int, List<HistoryAudioTrack>>> GetMappedAudio()
		{
			Dictionary<Audio.AudioType, Dictionary<int, List<HistoryAudioTrack>>> mappedAudio = new();
			foreach (HistoryAudioTrack historyAudioTrack in audioTracks)
			{
				if (!mappedAudio.TryGetValue(historyAudioTrack.audioType, out var audioLayers))
					mappedAudio[historyAudioTrack.audioType] = audioLayers = new();

				if (!audioLayers.TryGetValue(historyAudioTrack.layerNum, out var historyAudioTracks))
					audioLayers[historyAudioTrack.layerNum] = historyAudioTracks = new();

				historyAudioTracks.Add(historyAudioTrack);
			}

			return mappedAudio;
		}

		bool IsTrackInHistory(AudioTrack audioTrack, List<HistoryAudioTrack> historyAudioTracks)
		{
			bool isInHistory = false;
			foreach (HistoryAudioTrack historyAudioTrack in historyAudioTracks)
			{
				if (string.Equals(historyAudioTrack.name, audioTrack.Name, System.StringComparison.OrdinalIgnoreCase))
				{
					isInHistory = true;
					break;
				}
			}

			return isInHistory;
		}
	}
}
