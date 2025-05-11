using System.Collections;
using System.Threading.Tasks;
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

		Coroutine transitionProcess;
		RenderTexture renderTexture;
		string graphicName;
		bool isImage;
		bool hasAudio;

		public int Depth => depth;

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

		public async Task SetImageInstant(string name)
		{
			if (isImage && graphicName == name) return;

			Sprite sprite = await layerGroup.Manager.FileManager.LoadBackgroundImage(name);
			if (sprite == null) return;

			StopProcess();
			ApplyImage(name, sprite);
		}

		public Coroutine SetImage(string name, float speed = 0)
		{
			if (isImage && graphicName == name) return null;

			bool isSkipped = StopProcess();

			transitionProcess = layerGroup.Manager.StartCoroutine(ChangeGraphic(name, speed, isSkipped, true));
			return transitionProcess;
		}

		public void SetVideoInstant(string name, bool useAudio = true)
		{
			StopProcess();
			ApplyVideo(name, useAudio);
		}

		public Coroutine SetVideo(string name, bool useAudio = true, float speed = 0)
		{
			bool isSkipped = StopProcess();

			transitionProcess = layerGroup.Manager.StartCoroutine(ChangeGraphic(name, speed, isSkipped, false, useAudio));
			return transitionProcess;
		}

		void ApplyImage(string name, Sprite sprite)
		{
			if (sprite == null) return;

			ClearCurrentGraphic();

			isImage = true;
			hasAudio = false;
			graphicName = name;
			rawImage.texture = sprite.texture;
		}

		void ApplyVideo(string name, bool useAudio)
		{
			ClearCurrentGraphic();

			isImage = false;
			hasAudio = useAudio;
			graphicName = name;

			audioSource.mute = !useAudio;
			audioSource.volume = 0;

			videoPlayer.url = layerGroup.Manager.FileManager.GetVideoUrl(name);
			videoPlayer.frame = 0;
			videoPlayer.prepareCompleted += OnVideoPrepared;
			videoPlayer.Prepare();
		}

		IEnumerator ChangeGraphic(string name, float speed, bool isSkipped, bool isImage, bool useAudio = false)
		{
			Task<Sprite> imageTask = isImage ? layerGroup.Manager.FileManager.LoadBackgroundImage(name) : null;
			float fadeSpeed = GetTransitionSpeed(speed, isSkipped);

			if (graphicName != null)
			{
				fadeSpeed *= 2;
				yield return FadeGraphic(canvasGroup, false, fadeSpeed);
			}

			yield return new WaitUntil(() => !isImage || (imageTask != null && imageTask.IsCompleted));

			if (isImage && imageTask.IsCompletedSuccessfully && imageTask.Result != null)
				ApplyImage(name, imageTask.Result);
			else if (!isImage)
				ApplyVideo(name, useAudio);

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

				float transitionValue = Mathf.Lerp(startAlpha, targetAlpha, smoothPercentage);
				canvasGroup.alpha = transitionValue;

				if (!isImage && hasAudio)
					audioSource.volume = transitionValue;
				
				yield return null;
			}

			canvasGroup.alpha = targetAlpha;
		}
		
		void ClearCurrentGraphic()
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

			graphicName = null;
		}

		float GetTransitionSpeed(float speedInput, bool isTransitionSkipped)
		{
			if (isTransitionSkipped)
				return layerGroup.Manager.GameOptions.General.SkipTransitionSpeed;
			else if (speedInput <= 0)
				return layerGroup.Manager.GameOptions.BackgroundLayers.TransitionSpeed;
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

		void OnVideoPrepared(VideoPlayer videoPlayer)
		{
			this.videoPlayer.prepareCompleted -= OnVideoPrepared;

			RenderTexture texture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 0);
			rawImage.texture = texture;
			this.videoPlayer.targetTexture = texture;

			if (transitionProcess == null && hasAudio)
				audioSource.volume = 1f;

			this.videoPlayer.Play();
		}

		void OnVideoError(VideoPlayer source, string message)
		{
			Debug.LogWarning($"Video failed to load. {message}");
		}
	}
}
