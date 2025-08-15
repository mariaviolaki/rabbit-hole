using Dialogue;
using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Visuals
{
	public class VisualGroupManager : MonoBehaviour
	{
		[SerializeField] VisualLayerGroup[] layerGroups;
		[SerializeField] FileManagerSO fileManager;
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] DialogueManager dialogueManager;

		Dictionary<VisualType, VisualLayerGroup> layerGroupBank = new();

		public FileManagerSO FileManager => fileManager;
		public GameOptionsSO GameOptions => gameOptions;
		public DialogueManager Dialogue => dialogueManager;

		public Dictionary<VisualType, VisualLayerGroup> VisualGroups => layerGroupBank;

		void Awake()
		{
			foreach (VisualLayerGroup group in layerGroups)
			{
				group.Initialize(this);
				layerGroupBank.Add(group.Type, group);
			}
		}

		public VisualLayerGroup GetLayerGroup(VisualType visualType)
		{
			layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup);
			return visualLayerGroup;
		}

		public float GetTransitionSpeed(float speedInput, bool isTransitionSkipped)
		{
			if (isTransitionSkipped || dialogueManager.ReadMode == DialogueReadMode.Skip)
				return gameOptions.General.SkipTransitionSpeed;
			else if (speedInput <= 0)
				return gameOptions.BackgroundLayers.TransitionSpeed;
			else
				return speedInput;
		}

		public void Create(VisualType visualType, int count = 0)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return;

			visualLayerGroup.CreateLayers(count);
		}

		public void Clear(VisualType visualType, int depth = -1, bool isImmediate = false, float speed = 0)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return;

			visualLayerGroup.Clear(depth, isImmediate, speed);
		}

		public IEnumerator SetImage(VisualType visualType, int layerDepth, string name, bool isImmediate = false, float speed = 0f)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) yield break;

			VisualLayer visualLayer = visualLayerGroup.GetLayer(layerDepth);
			if (visualLayer == null) yield break;

			yield return fileManager.LoadBackgroundImage(name);
			Sprite sprite = fileManager.GetBackgroundImage(name);
			if (sprite == null) yield break;

			visualLayer.SetImage(sprite, name, isImmediate, speed);
		}

		public IEnumerator SetVideo(VisualType visualType, int layerDepth, string name, float volume = 0.5f, bool isMuted = false, bool isImmediate = false, float speed = 0f)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) yield break;

			VisualLayer visualLayer = visualLayerGroup.GetLayer(layerDepth);
			if (visualLayer == null) yield break;

			string path = fileManager.GetVideoUrl(name);
			if (path == null) yield break;

			yield return visualLayer.SetVideo(path, name, volume, isMuted, isImmediate, speed);
		}

		public void SkipTransition(VisualType visualType, int layerDepth)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return;

			visualLayerGroup.SkipTransitions(layerDepth);
		}

		public bool IsTransitioning(VisualType visualType, int layerDepth)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return false;

			return visualLayerGroup.IsTransitioning(layerDepth);
		}
	}
}
