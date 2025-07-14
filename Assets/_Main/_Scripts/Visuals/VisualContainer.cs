using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Visuals
{
	public class VisualContainer
	{
		const float FadeSpeedMultiplier = 10f;

		readonly VisualLayer visualLayer;
		readonly CanvasGroup canvasGroup;
		readonly RawImage rawImage;
		readonly VideoPlayer videoPlayer;
		readonly AudioSource audioSource;

		RenderTexture renderTexture;

		public VisualContainer(VisualLayer visualLayer, GameObject layerObject)
		{
			this.visualLayer = visualLayer;

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

		public void ClearImmediate()
		{
			UnloadVisual();
			canvasGroup.alpha = 0;
		}

		public IEnumerator Clear(bool isImmediate, float speed)
		{
			if (isImmediate)
			{
				ClearImmediate();
				yield break;
			}
			else
			{
				yield return ClearVisual(speed);
			}
		}

		public IEnumerator SetImage(string name, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (isImmediate)
				yield return SetImageImmediate(name);
			else
				yield return ChangeVisual(name, fadeSpeed, true, true);
		}

		public IEnumerator SetVideo(string name, bool isMuted = false, bool isImmediate = false, float speed = 0)
		{
			if (isImmediate)
			{
				ApplyVideo(name, isMuted);
				yield break;
			}
			else
			{
				yield return ChangeVisual(name, speed, false, isMuted);
			}
		}

		void ApplyImage(Sprite sprite)
		{
			if (sprite == null) return;

			UnloadVisual();

			rawImage.texture = sprite.texture;
		}

		void ApplyVideo(string name, bool isMuted)
		{
			UnloadVisual();

			audioSource.mute = isMuted;
			audioSource.volume = 0;

			videoPlayer.url = visualLayer.LayerGroup.Manager.FileManager.GetVideoUrl(name);
			videoPlayer.prepareCompleted += OnVideoPrepared;
			videoPlayer.Prepare();
		}

		IEnumerator SetImageImmediate(string name)
		{
			yield return visualLayer.LayerGroup.Manager.FileManager.LoadBackgroundImage(name);
			Sprite sprite = visualLayer.LayerGroup.Manager.FileManager.GetBackgroundImage(name);
			if (sprite == null) yield break;

			ApplyImage(sprite);
			canvasGroup.alpha = 1f;
		}

		IEnumerator ChangeVisual(string name, float speed, bool isImage, bool isMuted)
		{
			if (isImage)
			{
				yield return visualLayer.LayerGroup.Manager.FileManager.LoadBackgroundImage(name);
				Sprite sprite = visualLayer.LayerGroup.Manager.FileManager.GetBackgroundImage(name);
				if (sprite != null)
					ApplyImage(sprite);
			}
			else
			{
				ApplyVideo(name, isMuted);
			}

			yield return FadeVisual(true, speed);
		}

		IEnumerator ClearVisual(float speed)
		{
			yield return FadeVisual(false, speed);
			UnloadVisual();
		}

		IEnumerator FadeVisual(bool isFadeIn, float speed)
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

				if (!visualLayer.IsImage && !visualLayer.IsMuted)
					audioSource.volume = transitionValue;

				yield return null;
			}

			canvasGroup.alpha = targetAlpha;
		}

		void UnloadVisual()
		{
			if (visualLayer.VisualName == null) return;

			if (visualLayer.IsImage)
			{
				visualLayer.LayerGroup.Manager.FileManager.UnloadBackgroundImage(visualLayer.VisualName);
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
		}

		void OnVideoPrepared(VideoPlayer videoPlayer)
		{
			this.videoPlayer.prepareCompleted -= OnVideoPrepared;

			renderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 0);
			rawImage.texture = renderTexture;
			this.videoPlayer.targetTexture = renderTexture;

			if (visualLayer.IsImmediate)
			{
				audioSource.volume = visualLayer.IsMuted ? 0f : 1f;
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
