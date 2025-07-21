using Dialogue;
using IO;
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

		public Dictionary<VisualType, VisualLayerGroup> VisualGroups => layerGroupBank;

		void Awake()
		{
			foreach (VisualLayerGroup group in layerGroups)
			{
				group.Init(this);
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

		public bool StopProcess(ref Coroutine process)
		{
			if (process == null) return false;

			StopCoroutine(process);
			process = null;

			return true;
		}

		public void Create(VisualType visualType, int count = 0)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return;

			visualLayerGroup.CreateLayers(count);
		}

		public Coroutine Clear(VisualType visualType, int depth = -1, bool isImmediate = false, float speed = 0)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return null;

			return visualLayerGroup.Clear(depth, isImmediate, speed);
		}

		public Coroutine SetImage(VisualType visualType, int layerDepth, string name, bool isImmediate = false, float speed = 0)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return null;

			VisualLayer visualLayer = visualLayerGroup.GetLayer(layerDepth);
			if (visualLayer == null) return null;

			return visualLayer.SetImage(name, isImmediate, speed);
		}

		public Coroutine SetVideo(VisualType visualType, int layerDepth, string name, bool isMuted = false, bool isImmediate = false, float speed = 0)
		{
			if (!layerGroupBank.TryGetValue(visualType, out VisualLayerGroup visualLayerGroup)) return null;

			VisualLayer visualLayer = visualLayerGroup.GetLayer(layerDepth);
			if (visualLayer == null) return null;

			return visualLayer.SetVideo(name, isMuted, isImmediate, speed);
		}
	}
}
