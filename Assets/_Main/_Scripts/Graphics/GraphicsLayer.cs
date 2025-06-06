using System.Collections;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Graphics
{
	public class GraphicsLayer
	{
		const float FadeSpeedMultiplier = 10f;

		readonly GraphicsLayerGroup layerGroup;
		readonly CanvasGroup canvasGroup;
		readonly RawImage rawImage;
		readonly VideoPlayer videoPlayer;
		readonly AudioSource audioSource;
		readonly int depth;

		Coroutine transitionCoroutine;
		RenderTexture renderTexture;
		string graphicName;
		bool isImage;
		bool isMuted;

		public int Depth => depth;
		public bool IsIdle => transitionCoroutine == null;

		public GraphicsLayer(GraphicsLayerGroup layerGroup, GameObject layerObject, int depth)
		{
			this.layerGroup = layerGroup;
			this.depth = depth;

			RectTransform rectTransform = layerObject.GetComponent<RectTransform>();
			canvasGroup = layerObject.AddComponent<CanvasGroup>();
			rawImage = layerObject.AddComponent<RawImage>();
			videoPlayer = layerObject.AddComponent<VideoPlayer>();
			audioSource = layerObject.AddComponent<AudioSource>();

			videoPlayer.SetTargetAudioSource(0, audioSource);
			videoPlayer.source = VideoSource.Url;
			videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
			videoPlayer.isLooping = true;
			videoPlayer.errorReceived += OnVideoError;

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

		public void ClearInstant()
		{
			layerGroup.Manager.StopProcess(ref transitionCoroutine);
			UnloadGraphic();
			canvasGroup.alpha = 0;
		}

		public Coroutine Clear(float speed)
		{
			layerGroup.Manager.StopProcess(ref transitionCoroutine);
			transitionCoroutine = layerGroup.Manager.StartCoroutine(ClearGraphic(speed));
			return transitionCoroutine;
		}

		public void SetImageInstant(string name)
		{
			if (isImage && graphicName == name) return;

			layerGroup.Manager.StartCoroutine(LoadImage(name));
		}

		public Coroutine SetImage(string name, float speed = 0)
		{
			if (isImage && graphicName == name) return null;

			bool isSkipped = layerGroup.Manager.StopProcess(ref transitionCoroutine);

			transitionCoroutine = layerGroup.Manager.StartCoroutine(ChangeGraphic(name, speed, isSkipped, true, true));
			return transitionCoroutine;
		}

		public void SetVideoInstant(string name, bool isMuted = false)
		{
			layerGroup.Manager.StopProcess(ref transitionCoroutine);
			ApplyVideo(name, isMuted);
		}

		public Coroutine SetVideo(string name, bool isMuted = false, float speed = 0)
		{
			bool isSkipped = layerGroup.Manager.StopProcess(ref transitionCoroutine);

			transitionCoroutine = layerGroup.Manager.StartCoroutine(ChangeGraphic(name, speed, isSkipped, false, isMuted));
			return transitionCoroutine;
		}

		public bool StopTransition()
		{
			return layerGroup.Manager.StopProcess(ref transitionCoroutine);
		}

		void ApplyImage(string name, Sprite sprite)
		{
			if (sprite == null) return;

			UnloadGraphic();

			isImage = true;
			isMuted = true;
			graphicName = name;
			rawImage.texture = sprite.texture;
		}

		void ApplyVideo(string name, bool isMuted)
		{
			UnloadGraphic();

			isImage = false;
			this.isMuted = isMuted;
			graphicName = name;

			audioSource.mute = isMuted;
			audioSource.volume = 0;

			videoPlayer.url = layerGroup.Manager.FileManager.GetVideoUrl(name);
			videoPlayer.prepareCompleted += OnVideoPrepared;
			videoPlayer.Prepare();
		}

		IEnumerator LoadImage(string name)
		{
			yield return layerGroup.Manager.FileManager.LoadBackgroundImage(name);
			Sprite sprite = layerGroup.Manager.FileManager.GetBackgroundImage(name);
			if (sprite == null) yield break;

			layerGroup.Manager.StopProcess(ref transitionCoroutine);

			ApplyImage(name, sprite);
			canvasGroup.alpha = 1f;
		}

		IEnumerator ChangeGraphic(string name, float speed, bool isSkipped, bool isImage, bool isMuted)
		{
			float fadeSpeed = layerGroup.Manager.GetTransitionSpeed(speed, isSkipped);

			if (graphicName != null)
			{
				fadeSpeed *= 2;
				yield return FadeGraphic(false, fadeSpeed);
			}

			if (isImage)
			{
				yield return layerGroup.Manager.FileManager.LoadBackgroundImage(name);
				Sprite sprite = layerGroup.Manager.FileManager.GetBackgroundImage(name);
				if (sprite != null)
					ApplyImage(name, sprite);
			}
			else
			{
				ApplyVideo(name, isMuted);
			}

			yield return FadeGraphic(true, fadeSpeed);
			transitionCoroutine = null;
		}

		IEnumerator FadeGraphic(bool isFadeIn, float speed)
		{
			float startAlpha = canvasGroup.alpha;
			float targetAlpha = isFadeIn ? 1f : 0f;

			float timeElapsed = 0;
			float duration = (1f / speed) * FadeSpeedMultiplier * Mathf.Abs(targetAlpha - startAlpha);
			while (timeElapsed < duration)
			{
				timeElapsed += Time.deltaTime;
				float smoothPercentage = Mathf.SmoothStep(0, 1, Mathf.Clamp01(timeElapsed / duration));

				float transitionValue = Mathf.Lerp(startAlpha, targetAlpha, smoothPercentage);
				canvasGroup.alpha = transitionValue;

				if (!isImage && !isMuted)
					audioSource.volume = transitionValue;
				
				yield return null;
			}

			canvasGroup.alpha = targetAlpha;
		}

		IEnumerator ClearGraphic(float speed)
		{
			yield return FadeGraphic(false, speed);

			UnloadGraphic();
			transitionCoroutine = null;
		}
		
		void UnloadGraphic()
		{
			if (graphicName == null) return;

			if (isImage)
			{
				layerGroup.Manager.FileManager.UnloadBackgroundImage(graphicName);
			}
			else
			{
				videoPlayer.Stop();
				videoPlayer.url = null;
				videoPlayer.targetTexture = null;

				if (renderTexture != null)
				{
					renderTexture.Release();
					Object.Destroy(renderTexture);
				}
			}

			rawImage.texture = null;
			graphicName = null;
		}

		void OnVideoPrepared(VideoPlayer videoPlayer)
		{
			this.videoPlayer.prepareCompleted -= OnVideoPrepared;

			renderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 0);
			rawImage.texture = renderTexture;
			this.videoPlayer.targetTexture = renderTexture;

			if (transitionCoroutine == null)
			{
				audioSource.volume = isMuted ? 0f : 1f;
				canvasGroup.alpha = 1f;
			}

			this.videoPlayer.Play();
		}

		void OnVideoError(VideoPlayer source, string message)
		{
			Debug.LogWarning($"Video failed to load. {message}");
		}
	}
}
