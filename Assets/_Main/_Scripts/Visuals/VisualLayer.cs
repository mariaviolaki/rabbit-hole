using System.Collections;
using UnityEngine;

namespace Visuals
{
	public class VisualLayer : MonoBehaviour
	{
		const string primaryContainerName = "Primary";
		const string secondaryContainerName = "Secondary";

		int depth;
		VisualLayerGroup layerGroup;
		GameObject primaryGameObject;
		GameObject secondaryGameObject;
		VisualContainer primaryContainer;
		VisualContainer secondaryContainer;

		string visualName;
		bool isImage;
		float volume;
		bool isMuted;
		bool isImmediate;

		public bool IsTransitioning => primaryContainer.IsTransitioning || secondaryContainer.IsTransitioning;
		public VisualLayerGroup LayerGroup => layerGroup;
		public string VisualName => visualName;
		public bool IsImage => isImage;
		public float Volume => volume;
		public bool IsMuted => isMuted;
		public bool IsImmediate => isImmediate;
		public int Depth => depth;

		void Update()
		{
			primaryContainer.TransitionVisual();
			secondaryContainer.TransitionVisual();
		}

		public void Initialize(VisualLayerGroup layerGroup, int depth)
		{
			this.depth = depth;
			this.layerGroup = layerGroup;

			// Add the layer as child under the layer group
			int lastIndex = layerGroup.Root.childCount;
			int siblingIndex = lastIndex - depth;
			transform.SetParent(layerGroup.Root, false);
			transform.SetSiblingIndex(siblingIndex);

			RectTransform rectTransform = GetComponent<RectTransform>();
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;

			primaryGameObject = new GameObject(primaryContainerName, typeof(RectTransform));
			primaryGameObject.transform.SetParent(transform, false);
			primaryContainer = new VisualContainer(this, primaryGameObject);
			primaryContainer.OnCompleteTransition += SwapContainers;

			// Create a secondary visual container for smooth transitions
			secondaryGameObject = new GameObject(secondaryContainerName, typeof(RectTransform));
			secondaryGameObject.transform.SetParent(transform, false);
			secondaryContainer = new VisualContainer(this, secondaryGameObject);
			secondaryContainer.OnCompleteTransition += SwapContainers;

			secondaryGameObject.SetActive(false);
		}

		public void Clear(bool isImmediate = false, float speed = 0f)
		{
			if (primaryContainer.IsHidden) return;

			primaryContainer.Clear(isImmediate, speed);

			this.visualName = null;
			this.isImmediate = isImmediate;
		}

		public void SetImage(Sprite sprite, string name, bool isImmediate = false, float speed = 0f)
		{
			if (isImage && visualName == name) return;

			if (isImmediate)
			{
				primaryContainer.SetImage(sprite, name, true);
			}
			else
			{
				secondaryGameObject.SetActive(true);
				secondaryContainer.SetImage(sprite, name, false, speed);

				if (!primaryContainer.IsHidden)
					primaryContainer.Clear(false, speed);
			}

			this.isImage = true;
			this.visualName = name;
			this.isImmediate = isImmediate;
		}

		public IEnumerator SetVideo(string path, string name, float volume = 0.5f, bool isMuted = false, bool isImmediate = false, float speed = 0f)
		{
			if (!isImage && visualName == name) yield break;

			if (isImmediate)
			{
				yield return primaryContainer.SetVideo(path, name, volume, isMuted, true);
			}
			else
			{
				secondaryGameObject.SetActive(true);
				yield return secondaryContainer.SetVideo(path, name, volume, isMuted, false, speed);

				if (!primaryContainer.IsHidden)
					primaryContainer.Clear(false, speed);
			}

			this.isImage = false;
			this.volume = volume;
			this.isMuted = isMuted;
			this.visualName = name;
			this.isImmediate = isImmediate;
		}

		public void SkipTransition()
		{
			primaryContainer.SkipTransition();
			secondaryContainer.SkipTransition();
		}

		void SwapContainers(VisualContainer container)
		{
			if (container == primaryContainer) return;

			GameObject tempGameObject = primaryGameObject;
			VisualContainer tempContainer = primaryContainer;

			primaryGameObject = secondaryGameObject;
			primaryContainer = secondaryContainer;

			secondaryGameObject = tempGameObject;
			secondaryContainer = tempContainer;

			secondaryGameObject.SetActive(false);
		}
	}
}
