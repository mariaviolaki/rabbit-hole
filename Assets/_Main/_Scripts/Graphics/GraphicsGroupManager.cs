using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
	public class GraphicsGroupManager : MonoBehaviour
	{
		[SerializeField] GraphicsLayerGroup[] layerGroups;
		[SerializeField] FileManagerSO fileManager;
		[SerializeField] GameOptionsSO gameOptions;

		public static string BackgroundName = "Background";
		public static string ForegroundName = "Foreground";
		public static string CinematicName = "Cinematic";

		Dictionary<string, GraphicsLayerGroup> layerGroupDirectory = new Dictionary<string, GraphicsLayerGroup>();

		public FileManagerSO FileManager => fileManager;
		public GameOptionsSO GameOptions => gameOptions;

		void Awake()
		{
			foreach (GraphicsLayerGroup group in layerGroups)
			{
				group.Init(this);
				layerGroupDirectory.Add(group.Name, group);
			}
		}

		public GraphicsLayerGroup GetLayerGroup(string name)
		{
			if (!layerGroupDirectory.ContainsKey(name)) return null;

			return layerGroupDirectory[name];
		}

		public float GetTransitionSpeed(float speedInput, bool isTransitionSkipped)
		{
			if (isTransitionSkipped)
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
