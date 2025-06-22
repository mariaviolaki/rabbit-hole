using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Visuals
{
	public class VisualContainer
	{
		const float FadeSpeedMultiplier = 10f;

		readonly VisualLayerGroup layerGroup;
		readonly CanvasGroup canvasGroup;
		readonly RawImage rawImage;
		readonly VideoPlayer videoPlayer;
		readonly AudioSource audioSource;

		Coroutine transitionCoroutine;
		RenderTexture renderTexture;
		string visualName;
		bool isImage;
		bool isMuted;

		public bool IsIdle => transitionCoroutine == null;
		public string VisualName => visualName;
		public bool IsImage => isImage;
		public bool IsMuted => isMuted;

		public VisualContainer(VisualLayerGroup layerGroup, GameObject layerObject)
		{
			this.layerGroup = layerGroup;

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

			// Setup how the layer graphic will be displayed
			canvasGroup.alpha = 0;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
		}

		public Coroutine Clear(bool isImmediate = false, float speed = 0)
		{
			bool isSkipped = layerGroup.Manager.StopProcess(ref transitionCoroutine);

			if (isImmediate)
			{
				UnloadVisual();
				canvasGroup.alpha = 0;
				return null;
			}
			else
			{
				transitionCoroutine = layerGroup.Manager.StartCoroutine(ClearVisual(speed, isSkipped));
				return transitionCoroutine;
			}
		}

		public Coroutine SetImage(string name, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (isImage && visualName == name) return null;

			bool isSkipped = layerGroup.Manager.StopProcess(ref transitionCoroutine);

			if (isImmediate)
			{
				layerGroup.Manager.StartCoroutine(LoadImage(name));
				return null;
			}
			else
			{
				transitionCoroutine = layerGroup.Manager.StartCoroutine(ChangeVisual(name, fadeSpeed, isSkipped, true, true));
				return transitionCoroutine;
			}
		}

		public Coroutine SetVideo(string name, bool isMuted = false, bool isImmediate = false, float speed = 0)
		{
			bool isSkipped = layerGroup.Manager.StopProcess(ref transitionCoroutine);

			if (isImmediate)
			{
				ApplyVideo(name, isMuted);
				return null;
			}
			else
			{
				transitionCoroutine = layerGroup.Manager.StartCoroutine(ChangeVisual(name, speed, isSkipped, false, isMuted));
				return transitionCoroutine;
			}
		}

		public bool StopTransition()
		{
			return layerGroup.Manager.StopProcess(ref transitionCoroutine);
		}

		void ApplyImage(string name, Sprite sprite)
		{
			if (sprite == null) return;

			UnloadVisual();

			isImage = true;
			isMuted = true;
			visualName = name;
			rawImage.texture = sprite.texture;
		}

		void ApplyVideo(string name, bool isMuted)
		{
			UnloadVisual();

			isImage = false;
			this.isMuted = isMuted;
			visualName = name;

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

		IEnumerator ChangeVisual(string name, float speed, bool isSkipped, bool isImage, bool isMuted)
		{
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

			yield return FadeVisual(true, speed, isSkipped);
			transitionCoroutine = null;
		}

		IEnumerator FadeVisual(bool isFadeIn, float speed, bool isSkipped)
		{
			speed = layerGroup.Manager.GetTransitionSpeed(speed, isSkipped);

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

		IEnumerator ClearVisual(float speed, bool isSkipped)
		{
			yield return FadeVisual(false, speed, isSkipped);

			UnloadVisual();
			transitionCoroutine = null;
		}

		void UnloadVisual()
		{
			if (visualName == null) return;

			if (isImage)
			{
				layerGroup.Manager.FileManager.UnloadBackgroundImage(visualName);
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
			visualName = null;
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
