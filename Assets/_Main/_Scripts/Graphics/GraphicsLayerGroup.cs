using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphics
{
	[System.Serializable]
	public class GraphicsLayerGroup
	{
		[SerializeField] string name;
		[SerializeField] RectTransform root;

		GraphicsGroupManager manager;
		Dictionary<int, GraphicsLayer> layers = new();
		Coroutine clearCoroutine;

		public string Name => name;
		public RectTransform Root => root;
		public GraphicsGroupManager Manager => manager;

		public void Init(GraphicsGroupManager manager)
		{
			this.manager = manager;
		}

		public void ClearInstant(int depth = -1)
		{
			GraphicsLayer[] layersToClear = GetLayersFromDepth(depth);
			if (layersToClear == null) return;

			foreach (GraphicsLayer layer in layersToClear)
			{
				layer.ClearInstant();
			}
		}

		public Coroutine Clear(int depth = -1, float speed = 0)
		{
			GraphicsLayer[] layersToClear = GetLayersFromDepth(depth);
			if (layersToClear == null) return null;

			bool isAnyProcessStopped = false;
			foreach (GraphicsLayer layer in layersToClear)
			{
				if (layer.StopTransition())
					isAnyProcessStopped = true;
			}

			bool isClearStopped = manager.StopProcess(ref clearCoroutine);
			speed = manager.GetTransitionSpeed(speed, isClearStopped || isAnyProcessStopped);

			clearCoroutine = manager.StartCoroutine(ClearLayers(layersToClear, speed));
			return clearCoroutine;
		}

		public GraphicsLayer GetLayer(int depth)
		{
			if (!layers.ContainsKey(depth))
			{
				Debug.LogError($"Layer {depth} not found in {name} layer group.");
				return null;
			}

			return layers[depth];
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
				Debug.LogWarning($"Layer {clampedIndex} already exists in {name} layer group.");
				return;
			}

			GameObject layerObject = new GameObject($"Layer {clampedIndex}", typeof(RectTransform));
			GraphicsLayer layer = new GraphicsLayer(this, layerObject, clampedIndex);
			layers[clampedIndex] = layer;

			if (depth != clampedIndex)
				Debug.LogWarning($"'Layer {depth}' was created as 'Layer {clampedIndex}' in {name} layer group because it was out of bounds.");
		}

		GraphicsLayer[] GetLayersFromDepth(int depth)
		{
			if (depth < 0)
				return layers.Values.ToArray();

			GraphicsLayer layer = GetLayer(depth);
			if (layer == null) return null;

			return new GraphicsLayer[] { GetLayer(depth) };
		}

		IEnumerator ClearLayers(GraphicsLayer[] layersToClear, float speed)
		{
			foreach (GraphicsLayer layer in layersToClear)
			{
				layer.Clear(speed);
			}

			yield return new WaitUntil(() => layersToClear.All(l => l.IsIdle));
			clearCoroutine = null;
		}
	}
}
