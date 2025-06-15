using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
	public class AudioGroup
	{
		readonly AudioManager manager;
		readonly string name;
		readonly AudioType type;
		readonly Transform root;
		readonly AudioMixerGroup mixerGroup;
		readonly Dictionary<int, AudioLayer> layers = new();

		public AudioManager Manager => manager;
		public AudioMixerGroup MixerGroup => mixerGroup;
		public Transform Root => root;
		public AudioType Type => type;

		public AudioGroup(AudioManager manager, AudioType type, AudioMixerGroup mixerGroup)
		{
			this.manager = manager;
			this.type = type;
			this.mixerGroup = mixerGroup;

			name = type.ToString();
			root = new GameObject(name).transform;
		}

		public void Play(string audioName, float volume = 0.5f, float pitch = 1f, bool isLooping = false, bool isImmediate = false, float fadeSpeed = 0f, int layerNum = 0)
		{
			layerNum = GetValidLayerNum(layerNum);
			if (layerNum == -1) return;

			layers[layerNum].Play(audioName, volume, pitch, isLooping, isImmediate, fadeSpeed);
		}

		public void Stop(string audioName, bool isImmediate = false, float fadeSpeed = 0f, int layerNum = 0)
		{
			layerNum = GetValidLayerNum(layerNum);
			if (layerNum == -1) return;

			layers[layerNum].Stop(audioName, isImmediate, fadeSpeed);
		}

		public AudioLayer GetLayer(int num)
		{
			if (!layers.ContainsKey(num))
			{
				Debug.LogError($"Layer {num} not found in {name} audio group.");
				return null;
			}

			return layers[num];
		}

		public void CreateLayers(int count = 1)
		{
			// Prevent multiple initializations
			if (layers.Count > 0) return;

			// SFX play on a single layer
			count = type == AudioType.SFX ? 1 : Mathf.Max(1, count);

			for (int i = 0; i < count; i++)
			{
				CreateLayer(i);
			}
		}

		int GetValidLayerNum(int layerNum)
		{
			if (layers.Count == 0)
			{
				Debug.LogWarning($"No layers have been initialized for {name} Audio Group.");
				return -1;
			}

			// SFX play on a single layer
			return (type == AudioType.SFX) ? 0 : Mathf.Clamp(layerNum, 0, layers.Count - 1);
		}

		void CreateLayer(int depth)
		{
			int clampedIndex = Mathf.Clamp(depth, 0, layers.Count);

			if (layers.ContainsKey(clampedIndex))
			{
				Debug.LogWarning($"Layer {clampedIndex} already exists in {name} audio group.");
				return;
			}

			AudioLayer layer = new AudioLayer(this, clampedIndex);
			layers[clampedIndex] = layer;

			if (depth != clampedIndex)
				Debug.LogWarning($"'Layer {depth}' was created as 'Layer {clampedIndex}' in {name} audio group because it was out of bounds.");
		}
	}
}
