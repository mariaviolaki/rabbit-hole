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

		public void ClearInstant()
		{
			foreach (GraphicsLayer layer in layers.Values)
			{
				layer.ClearInstant();
			}
		}

		public Coroutine Clear(float speed = 0)
		{
			bool isAnyProcessStopped = false;
			foreach (GraphicsLayer layer in layers.Values)
			{
				if (layer.StopTransition())
					isAnyProcessStopped = true;
			}

			bool isClearStopped = manager.StopProcess(ref clearCoroutine);
			speed = manager.GetTransitionSpeed(speed, isClearStopped || isAnyProcessStopped);

			clearCoroutine = manager.StartCoroutine(ClearLayers(speed));
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

		public void CreateLayer(int depth)
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

		IEnumerator ClearLayers(float speed)
		{
			foreach (GraphicsLayer layer in layers.Values)
			{
				layer.Clear(speed);
			}

			yield return new WaitUntil(() => layers.Values.All(l => l.IsIdle));
			clearCoroutine = null;
		}
	}
}
