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

		public VisualType Type => visualType;
		public RectTransform Root => root;
		public VisualGroupManager Manager => manager;
		public Dictionary<int, VisualLayer> Layers => layers;

		public void Initialize(VisualGroupManager manager)
		{
			this.manager = manager;
		}

		public void Clear(int depth = -1, bool isImmediate = false, float speed = 0)
		{
			List<VisualLayer> selectedLayers = GetLayersFromDepth(depth);
			foreach (VisualLayer layer in selectedLayers)
			{
				layer.Clear(isImmediate, speed);
			}
		}

		public void SkipTransitions(int depth = -1)
		{
			List<VisualLayer> selectedLayers = GetLayersFromDepth(depth);
			foreach (VisualLayer layer in selectedLayers)
			{
				layer.SkipTransition();
			}
		}

		public bool IsTransitioning(int depth = -1)
		{
			List<VisualLayer> selectedLayers = GetLayersFromDepth(depth);
			foreach (VisualLayer layer in selectedLayers)
			{
				if (layer.IsTransitioning) return true;
			}

			return false;
		}

		public VisualLayer GetLayer(int depth)
		{
			if (layers.TryGetValue(depth, out VisualLayer layer)) return layer;

			Debug.LogWarning($"Layer {depth} not found in {visualType} layer group.");
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
				Debug.LogWarning($"Layer {clampedIndex} already exists in {visualType} layer group.");
				return;
			}

			GameObject layerObject = new GameObject($"Layer {clampedIndex}", typeof(RectTransform));
			VisualLayer layer = layerObject.AddComponent<VisualLayer>();
			layer.Initialize(this, clampedIndex);

			layers[clampedIndex] = layer;

			if (depth != clampedIndex)
				Debug.LogWarning($"'Layer {depth}' was created as 'Layer {clampedIndex}' in {visualType} layer group because it was out of bounds.");
		}

		List<VisualLayer> GetLayersFromDepth(int depth)
		{
			if (depth < 0) return layers.Values.ToList();

			List<VisualLayer> layersToClear = new();

			VisualLayer layerToClear = GetLayer(depth);
			if (layerToClear != null)
				layersToClear.Add(layerToClear);

			return layersToClear;
		}
	}
}
