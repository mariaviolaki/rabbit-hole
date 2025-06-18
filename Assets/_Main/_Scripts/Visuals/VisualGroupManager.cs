using Dialogue;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
	public class VisualGroupManager : MonoBehaviour
	{
		[SerializeField] VisualLayerGroup[] layerGroups;
		[SerializeField] FileManagerSO fileManager;
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] DialogueSystem dialogueSystem;

		public static string BackgroundName = "Background";
		public static string ForegroundName = "Foreground";
		public static string CinematicName = "Cinematic";

		Dictionary<string, VisualLayerGroup> layerGroupBank = new Dictionary<string, VisualLayerGroup>();

		public FileManagerSO FileManager => fileManager;
		public GameOptionsSO GameOptions => gameOptions;

		void Awake()
		{
			foreach (VisualLayerGroup group in layerGroups)
			{
				group.Init(this);
				layerGroupBank.Add(group.Name, group);
			}
		}

		public VisualLayerGroup GetLayerGroup(string name)
		{
			if (!layerGroupBank.ContainsKey(name)) return null;

			return layerGroupBank[name];
		}

		public float GetTransitionSpeed(float speedInput, bool isTransitionSkipped)
		{
			if (isTransitionSkipped || dialogueSystem.ReadMode == DialogueReadMode.Skip)
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
	}
}
