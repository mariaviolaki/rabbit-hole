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
		Coroutine skippedClearCoroutine;

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
			if (skippedClearCoroutine != null) return null;

			List<VisualLayer> layersToClear = GetLayersFromDepth(depth);
			if (layersToClear.Count == 0) return null;

			bool areLayersIdle = layersToClear.All(layer => layer.IsIdle);
			bool isSkipped = manager.StopProcess(ref clearCoroutine) || !areLayersIdle;

			if (isImmediate)
			{
				ClearLayersImmediate(layersToClear, isSkipped);
				return null;
			}
			else if (isSkipped)
			{
				skippedClearCoroutine = manager.StartCoroutine(ClearLayers(layersToClear, speed, isSkipped));
				return skippedClearCoroutine;
			}
			else
			{
				clearCoroutine = manager.StartCoroutine(ClearLayers(layersToClear, speed, isSkipped));
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

		void ClearLayersImmediate(List<VisualLayer> layersToClear, bool isSkipped)
		{
			foreach (VisualLayer layer in layersToClear)
			{
				layer.ClearInGroupImmediate(isSkipped);
			}
		}

		IEnumerator ClearLayers(List<VisualLayer> layersToClear, float speed, bool isSkipped)
		{
			List<IEnumerator> clearProcesses = new();
			foreach (VisualLayer layer in layersToClear)
			{
				clearProcesses.Add(layer.ClearInGroup(false, speed, isSkipped));
			}

			yield return Utilities.RunConcurrentProcesses(clearProcesses);
			if (isSkipped) skippedClearCoroutine = null;
			else clearCoroutine = null;
		}

		List<VisualLayer> GetLayersFromDepth(int depth)
		{
			List<VisualLayer> layersToClear = new();

			if (depth < 0)
			{
				foreach (VisualLayer layer in layers.Values)
				{
					if (!layer.IsBusy)
						layersToClear.Add(layer);
				}
				return layersToClear;
			}
			else
			{
				VisualLayer layerToClear = GetLayer(depth);
				if (layerToClear != null && !layerToClear.IsBusy)
					layersToClear.Add(layerToClear);
			}

			return layersToClear;
		}
	}
}
