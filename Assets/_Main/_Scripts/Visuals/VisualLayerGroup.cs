using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Visuals
{
	[System.Serializable]
	public class VisualLayerGroup
	{
		[SerializeField] VisualType visualType;
		[SerializeField] RectTransform root;

		readonly Dictionary<int, VisualLayer> layers = new();
		VisualGroupManager manager;
		Coroutine clearCoroutine;

		public VisualType Type => visualType;
		public RectTransform Root => root;
		public VisualGroupManager Manager => manager;
		public Dictionary<int, VisualLayer> Layers => layers;

		public void Init(VisualGroupManager manager)
		{
			this.manager = manager;
		}

		public Coroutine Clear(int depth = -1, bool isImmediate = false, float speed = 0)
		{
			VisualLayer[] layersToClear = GetLayersFromDepth(depth);
			if (layersToClear == null) return null;

			if (isImmediate)
			{
				foreach (VisualLayer layer in layersToClear)
				{
					layer.Clear(true);
				}
				return null;
			}
			else
			{
				bool isAnyProcessStopped = false;
				foreach (VisualLayer layer in layersToClear)
				{
					if (layer.StopTransition())
						isAnyProcessStopped = true;
				}

				bool isClearStopped = manager.StopProcess(ref clearCoroutine);
				speed = manager.GetTransitionSpeed(speed, isClearStopped || isAnyProcessStopped);

				clearCoroutine = manager.StartCoroutine(ClearLayers(layersToClear, speed));
				return clearCoroutine;
			}
		}

		public VisualLayer GetLayer(int depth)
		{
			if (layers.TryGetValue(depth, out VisualLayer layer)) return layer;

			Debug.LogWarning($"Layer {depth} not found in {visualType.ToString()} layer group.");
			return null;
		}

		public void CreateLayers(int count = 0)
		{
			// Prevent multiple initializations
			if (layers.Count > 0) return;

			count = Mathf.Max(1, count);

			for (int i = 0; i < count; i++)
			{
				CreateLayer(i);
			}
		}

		void CreateLayer(int depth)
		{
			int clampedIndex = Mathf.Clamp(depth, 0, layers.Count);

			if (layers.ContainsKey(clampedIndex))
			{
				Debug.LogWarning($"Layer {clampedIndex} already exists in {visualType.ToString()} layer group.");
				return;
			}

			GameObject layerObject = new GameObject($"Layer {clampedIndex}", typeof(RectTransform));
			VisualLayer layer = new VisualLayer(this, layerObject, clampedIndex);
			layers[clampedIndex] = layer;

			if (depth != clampedIndex)
				Debug.LogWarning($"'Layer {depth}' was created as 'Layer {clampedIndex}' in {visualType.ToString()} layer group because it was out of bounds.");
		}

		VisualLayer[] GetLayersFromDepth(int depth)
		{
			if (depth < 0)
				return layers.Values.ToArray();

			VisualLayer layer = GetLayer(depth);
			if (layer == null) return null;

			return new VisualLayer[] { GetLayer(depth) };
		}

		IEnumerator ClearLayers(VisualLayer[] layersToClear, float speed)
		{
			foreach (VisualLayer layer in layersToClear)
			{
				layer.Clear(false, speed);
			}

			while (!layersToClear.All(l => l.IsIdle)) yield return null;

			clearCoroutine = null;
		}
	}
}
