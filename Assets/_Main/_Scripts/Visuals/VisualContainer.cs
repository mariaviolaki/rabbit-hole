using Characters;
using Dialogue;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.Video;
using VN;

namespace Visuals
{
	public class VisualContainer
	{
		readonly VisualLayer visualLayer;
		readonly DialogueFlowController flowController;
		readonly VNOptionsSO vnOptions;
		readonly CanvasGroup canvasGroup;
		readonly RawImage rawImage;
		readonly VideoPlayer videoPlayer;
		readonly AudioSource audioSource;

		RenderTexture renderTexture;

		const float TransitionSpeedMultiplier = 0.2f;
		float targetFade;
		float transitionSpeed;
		bool isPreparingVideo;
		bool hasVideoFailed;
		bool isImage;
		string visualName;
		float targetVolume;
		bool isMuted;
		TransitionStatus transitionStatus = TransitionStatus.Completed;

		public event Action<VisualContainer> OnCompleteTransition;

		public bool IsTransitioning => transitionStatus != TransitionStatus.Completed;
		public bool IsVisible => transitionStatus == TransitionStatus.Completed && Utilities.AreApproximatelyEqual(targetFade, 1f);
		public bool IsHidden => transitionStatus == TransitionStatus.Completed && Utilities.AreApproximatelyEqual(targetFade, 0f);

		public VisualContainer(VisualLayer visualLayer, GameObject layerObject)
		{
			this.visualLayer = visualLayer;
			this.flowController = visualLayer.LayerGroup.Manager.Dialogue.FlowController;
			this.vnOptions = visualLayer.LayerGroup.Manager.Options;

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

		public void Clear(bool isImmediate = false, float speed = 0)
		{
			targetFade = 0f;

			if (isImmediate)
			{
				ClearImmediate();
				transitionStatus = TransitionStatus.Completed;
				OnCompleteTransition?.Invoke(this);
			}
			else
			{
				SetTransitionSpeed(speed);
				transitionStatus = transitionStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}

		public void SetImage(Sprite sprite, string name, bool isImmediate = false, float speed = 0)
		{
			targetFade = 1f;

			if (isImmediate)
			{
				SetImageImmediate(sprite, name);
				transitionStatus = TransitionStatus.Completed;
				OnCompleteTransition?.Invoke(this);
			}
			else
			{
				ApplyImage(sprite, name);
				SetTransitionSpeed(speed);
				transitionStatus = transitionStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}

		public IEnumerator SetVideo(string path, string name, float volume = 0.5f, bool isMuted = false, bool isImmediate = false, float speed = 0)
		{
			targetFade = 1f;

			if (isImmediate)
			{
				yield return SetVideoImmediate(path, name, volume, isMuted);
				transitionStatus = TransitionStatus.Completed;
				OnCompleteTransition?.Invoke(this);
			}
			else
			{
				yield return ApplyVideo(path, name, volume, isMuted);
				SetTransitionSpeed(speed);
				transitionStatus = transitionStatus == TransitionStatus.Completed ? TransitionStatus.Started : TransitionStatus.Skipped;
			}
		}
	
		public void TransitionVisual()
		{
			if (transitionStatus == TransitionStatus.Completed) return;

			float speed = transitionSpeed * Time.deltaTime;

			// Fade in/out the visual
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetFade, speed);

			// Fade in/out the video audio if applicable
			if (!isImage && !isMuted && !Utilities.AreApproximatelyEqual(audioSource.volume, targetVolume))
			{
				audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, speed);

				if (Utilities.AreApproximatelyEqual(audioSource.volume, targetVolume))
					audioSource.volume = targetVolume;
			}

			if (Utilities.AreApproximatelyEqual(canvasGroup.alpha, targetFade))
			{
				canvasGroup.alpha = targetFade;

				// If this was a fade-out, unload the visual after hiding it
				if (targetFade < 0.5f)
					UnloadVisual();

				transitionStatus = TransitionStatus.Completed;
				OnCompleteTransition?.Invoke(this);
			}
		}

		public void SkipTransition()
		{
			if (transitionStatus == TransitionStatus.Completed) return;

			SetTransitionSpeed(vnOptions.General.SkipTransitionSpeed);
		}

		void ClearImmediate()
		{
			canvasGroup.alpha = 0f;
			UnloadVisual();
		}

		void SetImageImmediate(Sprite sprite, string name)
		{
			ApplyImage(sprite, name);
			canvasGroup.alpha = 1f;
		}

		IEnumerator SetVideoImmediate(string path, string name, float volume, bool isMuted)
		{
			yield return ApplyVideo(path, name, volume, isMuted);
			canvasGroup.alpha = 1f;
		}

		void ApplyImage(Sprite sprite, string name)
		{
			if (visualName != null) UnloadVisual();

			rawImage.texture = sprite.texture;
			isImage = true;
			visualName = name;
		}

		IEnumerator ApplyVideo(string path, string name, float volume, bool isMuted)
		{
			while (isPreparingVideo) yield return null;
			if (visualName != null) UnloadVisual();

			isPreparingVideo = true;
			hasVideoFailed = false;

			audioSource.mute = isMuted;
			audioSource.volume = 0;
			videoPlayer.url = path;

			videoPlayer.prepareCompleted += OnVideoPrepared;
			videoPlayer.Prepare();

			while (isPreparingVideo) yield return null;

			videoPlayer.prepareCompleted -= OnVideoPrepared;
			yield return null;

			while (!hasVideoFailed && (videoPlayer.width <= 0 || videoPlayer.height <= 0)) yield return null;

			if (hasVideoFailed)
			{
				UnloadVisual();
				yield break;
			}

			renderTexture = new RenderTexture((int)videoPlayer.width, (int)videoPlayer.height, 0);
			rawImage.texture = renderTexture;
			videoPlayer.targetTexture = renderTexture;

			if (visualLayer.IsImmediate)
			{
				audioSource.volume = isMuted ? 0f : volume;
				canvasGroup.alpha = 1f;
			}

			videoPlayer.Play();

			isImage = false;
			visualName = name;
			this.targetVolume = volume;
			this.isMuted = isMuted;
		}

		void UnloadVisual()
		{
			if (visualName == null) return;

			if (isImage)
			{
				AssetLabelReference assetLabel = visualLayer.LayerGroup.Type == VisualType.CG
					? visualLayer.LayerGroup.Manager.ImageLabel
					: visualLayer.LayerGroup.Manager.CGLabel;

				visualLayer.LayerGroup.Manager.Assets.UnloadImage(visualName, assetLabel);
			}
			else
			{
				videoPlayer.Stop();
				audioSource.Stop();
				videoPlayer.url = null;
				audioSource.clip = null;
				videoPlayer.targetTexture = null;

				if (renderTexture != null)
				{
					renderTexture.Release();
					UnityEngine.Object.Destroy(renderTexture);
				}
			}

			hasVideoFailed = false;
			isPreparingVideo = false;
			rawImage.texture = null;
			visualName = null;
		}

		void OnVideoPrepared(VideoPlayer videoPlayer)
		{
			isPreparingVideo = false;
		}

		void SetTransitionSpeed(float speed)
		{
			float baseSpeed = speed;

			if (flowController.IsSkipping || transitionStatus == TransitionStatus.Skipped)
				baseSpeed = vnOptions.General.SkipTransitionSpeed;
			else if (speed < Mathf.Epsilon)
				baseSpeed = vnOptions.Images.TransitionSpeed;

			this.transitionSpeed = baseSpeed * TransitionSpeedMultiplier;
		}

		void OnVideoError(VideoPlayer source, string message)
		{
			Debug.LogWarning($"Video failed to load. {message}");
			hasVideoFailed = true;
			isPreparingVideo = false;
		}
	}
}
