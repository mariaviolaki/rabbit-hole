using System.Collections;
using System.Collections.Generic;
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

		Coroutine visualCoroutine;
		Coroutine skippedVisualCoroutine;
		bool isSwappingContainers;

		public string VisualName => primaryContainer.VisualName;
		public bool IsImage => primaryContainer.IsImage;
		public bool IsMuted => primaryContainer.IsMuted;
		public int Depth => depth;
		public bool IsIdle => visualCoroutine == null && skippedVisualCoroutine == null;
		public bool IsBusy => skippedVisualCoroutine != null;

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
			if (skippedVisualCoroutine != null) return null;
			bool isSkipped = layerGroup.Manager.StopProcess(ref visualCoroutine);
			RestoreContainerAfterSkip(isSkipped);

			speed = layerGroup.Manager.GetTransitionSpeed(speed, isSkipped);
			if (isImmediate)
			{
				skippedVisualCoroutine = layerGroup.Manager.StartCoroutine(ClearVisual(isImmediate, speed, isSkipped));
				return skippedVisualCoroutine;
			}
			else if (isSkipped)
			{
				skippedVisualCoroutine = layerGroup.Manager.StartCoroutine(ClearVisual(isImmediate, speed, isSkipped));
				return skippedVisualCoroutine;
			}
			else
			{
				visualCoroutine = layerGroup.Manager.StartCoroutine(ClearVisual(isImmediate, speed, isSkipped));
				return visualCoroutine;
			}
		}

		public Coroutine SetImage(string name, bool isImmediate = false, float speed = 0)
		{
			if (skippedVisualCoroutine != null) return null;
			bool isSkipped = layerGroup.Manager.StopProcess(ref visualCoroutine);
			RestoreContainerAfterSkip(isSkipped);

			if (isImmediate)
			{
				skippedVisualCoroutine = layerGroup.Manager.StartCoroutine(SetImageImmediate(name));
				return skippedVisualCoroutine;
			}
			else if (isSkipped)
			{
				skippedVisualCoroutine = layerGroup.Manager.StartCoroutine(SwapImage(name, speed, isSkipped));
				return skippedVisualCoroutine;
			}
			else
			{
				visualCoroutine = layerGroup.Manager.StartCoroutine(SwapImage(name, speed, isSkipped));
				return visualCoroutine;
			}
		}

		public Coroutine SetVideo(string name, bool isMuted = false, bool isImmediate = false, float speed = 0)
		{
			if (skippedVisualCoroutine != null) return null;
			bool isSkipped = layerGroup.Manager.StopProcess(ref visualCoroutine);
			RestoreContainerAfterSkip(isSkipped);

			if (isImmediate)
			{
				primaryContainer.SetVideo(name, isMuted, true);
				return null;
			}
			else if (isSkipped)
			{
				skippedVisualCoroutine = layerGroup.Manager.StartCoroutine(SwapVideo(name, isMuted, speed, isSkipped));
				return skippedVisualCoroutine;
			}
			else
			{
				visualCoroutine = layerGroup.Manager.StartCoroutine(SwapVideo(name, isMuted, speed, isSkipped));
				return visualCoroutine;
			}
		}

		public void ClearInGroupImmediate(bool isGroupSkipped)
		{
			if (skippedVisualCoroutine != null) return;
			bool isSkipped = layerGroup.Manager.StopProcess(ref visualCoroutine) || isGroupSkipped;
			RestoreContainerAfterSkip(isSkipped);

			primaryContainer.ClearImmediate();
		}

		public IEnumerator ClearInGroup(bool isImmediate, float speed, bool isGroupSkipped)
		{
			if (skippedVisualCoroutine != null) yield break;
			bool isSkipped = layerGroup.Manager.StopProcess(ref visualCoroutine) || isGroupSkipped;
			RestoreContainerAfterSkip(isSkipped);

			speed = layerGroup.Manager.GetTransitionSpeed(speed, isSkipped);
			yield return primaryContainer.Clear(isImmediate, speed);
		}

		IEnumerator ClearVisual(bool isImmediate, float speed, bool isSkipped)
		{
			speed = layerGroup.Manager.GetTransitionSpeed(speed, isSkipped);
			yield return primaryContainer.Clear(isImmediate, speed);

			if (isSkipped) skippedVisualCoroutine = null;
			else visualCoroutine = null;
		}

		IEnumerator SetImageImmediate(string name)
		{
			yield return primaryContainer.SetImage(name, true);
			skippedVisualCoroutine = null;
		}

		IEnumerator SwapImage(string name, float speed, bool isSkipped)
		{
			speed = layerGroup.Manager.GetTransitionSpeed(speed, isSkipped);

			while (isSwappingContainers) yield return null;
			isSwappingContainers = true;

			ToggleSecondaryVisual(true);

			List<IEnumerator> processes = new()
			{
				secondaryContainer.SetImage(name, false, speed),
				primaryContainer.Clear(false, speed)
			};
			yield return Utilities.RunConcurrentProcesses(processes);

			ToggleSecondaryVisual(false);

			isSwappingContainers = false;
			if (isSkipped) skippedVisualCoroutine = null;
			else visualCoroutine = null;
		}

		IEnumerator SwapVideo(string name, bool isMuted, float speed, bool isSkipped)
		{
			speed = layerGroup.Manager.GetTransitionSpeed(speed, isSkipped);

			while (isSwappingContainers) yield return null;
			isSwappingContainers = true;

			ToggleSecondaryVisual(true);

			List<IEnumerator> processes = new()
			{
				secondaryContainer.SetVideo(name, isMuted, false, speed),
				primaryContainer.Clear(false, speed)
			};
			yield return Utilities.RunConcurrentProcesses(processes);

			ToggleSecondaryVisual(false);

			isSwappingContainers = false;
			if (isSkipped) skippedVisualCoroutine = null;
			else visualCoroutine = null;
		}

		void RestoreContainerAfterSkip(bool isSkipped)
		{
			if (!isSkipped || primaryGameObject.activeSelf) return;

			ToggleSecondaryVisual(false);
			isSwappingContainers = false;
		}

		void ToggleSecondaryVisual(bool isActive)
		{
			if (!isActive)
			{
				// Swap the visual containers because the graphic is now in the second one
				SwapContainers();
				secondaryContainer.ClearImmediate();
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
