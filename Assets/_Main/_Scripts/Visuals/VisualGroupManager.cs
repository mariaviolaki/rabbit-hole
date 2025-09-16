using Dialogue;
using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VN;

namespace Visuals
{
	public class VisualGroupManager : MonoBehaviour
	{
		[SerializeField] VisualLayerGroup[] layerGroups;
		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] DialogueManager dialogueManager;

		Dictionary<VisualType, VisualLayerGroup> layerGroupBank = new();

		public AssetManagerSO Assets => assetManager;
		public VNOptionsSO GameOptions => vnOptions;
		public DialogueManager Dialogue => dialogueManager;

		public Dictionary<VisualType, VisualLayerGroup> VisualGroups => layerGroupBank;

		void Awake()
		{
			foreach (VisualLayerGroup group in layerGroups)
			{
				group.Initialize(this);
				group.CreateLayers(vnOptions.Images.Layers);
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
				return vnOptions.General.SkipTransitionSpeed;
			else if (speedInput <= 0)
				return vnOptions.Images.TransitionSpeed;
			else
				return speedInput;
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

			yield return assetManager.LoadImage(name);
			Sprite sprite = assetManager.GetImage(name);
			if (sprite == null) yield break;

			visualLayer.SetImage(sprite, name, isImmediate, speed);
		}

		public IEnumerator SetVideo(VisualType visualType, int layerDepth, string name, float volume = 0.5f, bool isMuted = false, bool isImmediate = false, float speed = 0f)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) yield break;

			VisualLayer visualLayer = visualLayerGroup.GetLayer(layerDepth);
			if (visualLayer == null) yield break;

			string path = assetManager.GetVideoUrl(name);
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
