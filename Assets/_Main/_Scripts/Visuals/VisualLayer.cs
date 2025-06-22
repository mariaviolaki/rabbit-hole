using System.Collections;
using UnityEngine;

namespace Visuals
{
	public class VisualLayer
	{
		const string primaryContainerName = "Primary";
		const string secondaryContainerName = "Secondary";

		readonly int depth;
		readonly VisualLayerGroup layerGroup;

		GameObject primaryGameObject;
		GameObject secondaryGameObject;
		VisualContainer primaryContainer;
		VisualContainer secondaryContainer;
		Coroutine swapCoroutine;

		public string VisualName => primaryContainer.VisualName;
		public bool IsImage => primaryContainer.IsImage;
		public bool IsMuted => primaryContainer.IsMuted;
		public int Depth => depth;
		public bool IsIdle => swapCoroutine == null && primaryContainer.IsIdle;

		public VisualLayer(VisualLayerGroup layerGroup, GameObject layerObject, int depth)
		{
			this.depth = depth;
			this.layerGroup = layerGroup;

			// Add the layer as child under the layer group
			int lastIndex = layerGroup.Root.childCount;
			int siblingIndex = lastIndex - depth;
			layerObject.transform.SetParent(layerGroup.Root, false);
			layerObject.transform.SetSiblingIndex(siblingIndex);

			RectTransform rectTransform = layerObject.GetComponent<RectTransform>();
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;

			primaryGameObject = new GameObject(primaryContainerName, typeof(RectTransform));
			primaryGameObject.transform.SetParent(layerObject.transform, false);
			primaryContainer = new VisualContainer(layerGroup, primaryGameObject);

			// Create a secondary visual container for smooth transitions
			secondaryGameObject = new GameObject(secondaryContainerName, typeof(RectTransform));
			secondaryGameObject.transform.SetParent(layerObject.transform, false);
			secondaryContainer = new VisualContainer(layerGroup, secondaryGameObject);

			secondaryGameObject.SetActive(false);
		}

		public Coroutine Clear(bool isImmediate = false, float speed = 0)
		{
			layerGroup.Manager.StopProcess(ref swapCoroutine);
			return primaryContainer.Clear(isImmediate, speed);
		}

		public Coroutine SetImage(string name, bool isImmediate = false, float speed = 0)
		{
			if (primaryContainer.IsImage && primaryContainer.VisualName == name) return null;

			layerGroup.Manager.StopProcess(ref swapCoroutine);

			if (isImmediate)
			{
				primaryContainer.SetImage(name, true);
				return null;
			}
			else
			{
				swapCoroutine = layerGroup.Manager.StartCoroutine(SwapImage(name, speed));
				return swapCoroutine;
			}
		}

		public Coroutine SetVideo(string name, bool isMuted = false, bool isImmediate = false, float speed = 0)
		{
			if (!primaryContainer.IsImage && primaryContainer.VisualName == name) return null;

			layerGroup.Manager.StopProcess(ref swapCoroutine);

			if (isImmediate)
			{
				primaryContainer.SetVideo(name, isMuted, true);
				return null;
			}
			else
			{
				swapCoroutine = layerGroup.Manager.StartCoroutine(SwapVideo(name, isMuted, speed));
				return swapCoroutine;
			}	
		}

		public bool StopTransition()
		{
			return primaryContainer.StopTransition();
		}

		IEnumerator SwapImage(string name, float speed)
		{
			ToggleSecondaryVisual(true);

			secondaryContainer.SetImage(name, false, speed);
			primaryContainer.Clear(false, speed);

			while (!secondaryContainer.IsIdle || !primaryContainer.IsIdle) yield return null;

			ToggleSecondaryVisual(false);
			swapCoroutine = null;
		}

		IEnumerator SwapVideo(string name, bool isMuted, float speed)
		{
			ToggleSecondaryVisual(true);

			secondaryContainer.SetVideo(name, isMuted, false, speed);
			primaryContainer.Clear(false, speed);

			while (!secondaryContainer.IsIdle || !primaryContainer.IsIdle) yield return null;

			ToggleSecondaryVisual(false);
			swapCoroutine = null;
		}

		void ToggleSecondaryVisual(bool isActive)
		{
			if (!isActive)
			{
				// Swap the visual containers because the graphic is now in the second one
				SwapContainers();
				secondaryContainer.Clear(true);
			}

			secondaryGameObject.SetActive(isActive);
		}

		void SwapContainers()
		{
			GameObject tempGameObject = primaryGameObject;
			VisualContainer tempContainer = primaryContainer;

			primaryGameObject = secondaryGameObject;
			primaryContainer = secondaryContainer;

			secondaryGameObject = tempGameObject;
			secondaryContainer = tempContainer;
		}
	}
}
