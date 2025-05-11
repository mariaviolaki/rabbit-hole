using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Graphics
{
	public class GraphicsLayer
	{
		const float FadeSpeedMultiplier = 10f;

		readonly GraphicsLayerGroup layerGroup;
		readonly CanvasGroup canvasGroup;
		readonly RawImage rawImage;
		readonly int depth;

		Coroutine transitionProcess;
		string currentGraphicName;
		bool currentIsImage;

		public int Depth => depth;

		public GraphicsLayer(GraphicsLayerGroup layerGroup, GameObject layerObject, int depth)
		{
			this.layerGroup = layerGroup;
			this.depth = depth;

			RectTransform rectTransform = layerObject.GetComponent<RectTransform>();
			canvasGroup = layerObject.AddComponent<CanvasGroup>();
			rawImage = layerObject.AddComponent<RawImage>();

			// Add the layer as child under the layer group
			int lastIndex = layerGroup.Root.childCount;
			int siblingIndex = lastIndex - depth;
			layerObject.transform.SetParent(layerGroup.Root, false);
			layerObject.transform.SetSiblingIndex(siblingIndex);

			// Setup how the layer graphic will be displayed
			canvasGroup.alpha = 0;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
		}

		public async Task SetImageInstant(string name)
		{
			if (currentIsImage && currentGraphicName == name) return;

			Sprite sprite = await layerGroup.Manager.FileManager.LoadBackgroundImage(name);
			if (sprite == null) return;

			StopProcess();
			SetGraphic(name, sprite.texture, true);
		}

		public Coroutine SetImage(string name, float speed = 0)
		{
			if (currentIsImage && currentGraphicName == name) return null;

			bool isSkipped = StopProcess();

			transitionProcess = layerGroup.Manager.StartCoroutine(ChangeGraphic(name, speed, isSkipped, true));
			return transitionProcess;
		}

		IEnumerator ChangeGraphic(string name, float speed, bool isSkipped, bool isImage)
		{
			Task<Sprite> loadTask = layerGroup.Manager.FileManager.LoadBackgroundImage(name);
			float fadeSpeed = GetTransitionSpeed(speed, isSkipped);

			if (currentGraphicName != null)
			{
				fadeSpeed *= 2;
				yield return FadeGraphic(canvasGroup, false, fadeSpeed);
			}
				
			yield return new WaitUntil(() => loadTask.IsCompleted);

			if (loadTask.IsCompletedSuccessfully && loadTask.Result != null)
				SetGraphic(name, loadTask.Result.texture, isImage);

			yield return FadeGraphic(canvasGroup, true, fadeSpeed);
			transitionProcess = null;
		}

		IEnumerator FadeGraphic(CanvasGroup canvasGroup, bool isFadeIn, float speed)
		{
			float startAlpha = canvasGroup.alpha;
			float targetAlpha = isFadeIn ? 1f : 0f;

			float timeElapsed = 0;
			float duration = (1f / speed) * FadeSpeedMultiplier * Mathf.Abs(targetAlpha - startAlpha);
			while (timeElapsed < duration)
			{
				timeElapsed += Time.deltaTime;
				float smoothPercentage = Mathf.SmoothStep(0, 1, Mathf.Clamp01(timeElapsed / duration));

				canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothPercentage);
				yield return null;
			}

			canvasGroup.alpha = targetAlpha;
		}

		void SetGraphic(string name, Texture texture, bool isImage)
		{
			if (texture == null) return;

			if (currentGraphicName != null)
			{
				if (currentIsImage)
					layerGroup.Manager.FileManager.UnloadBackgroundImage(currentGraphicName);
			}

			currentIsImage = isImage;
			currentGraphicName = name;
			rawImage.texture = texture;
		}

		float GetTransitionSpeed(float speedInput, bool isTransitionSkipped)
		{
			if (isTransitionSkipped)
				return layerGroup.Manager.GameOptions.Characters.SkipTransitionSpeed;
			else if (speedInput <= 0)
				return layerGroup.Manager.GameOptions.Characters.FadeTransitionSpeed;
			else
				return speedInput;
		}

		public bool StopProcess()
		{
			if (transitionProcess == null) return false;

			layerGroup.Manager.StopCoroutine(transitionProcess);
			transitionProcess = null;

			return true;
		}
	}
}
