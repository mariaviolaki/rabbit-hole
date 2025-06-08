using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class FadeableUI : MonoBehaviour
	{
		[SerializeField] protected GameOptionsSO gameOptions;

		CanvasGroup canvasGroup;
		Coroutine fadeCoroutine;
		const float FadeSpeedMultiplier = 1f;

		public bool IsVisible => canvasGroup.alpha == 1f;
		public bool IsHidden => canvasGroup.alpha == 0f;

		virtual protected void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			HideInstant();
		}

		virtual protected void Start()
		{

		}

		virtual public void ShowInstant()
		{
			if (IsVisible) return;

			StopFadeCoroutine();
			canvasGroup.alpha = 1f;
		}

		virtual public Coroutine Show(float fadeSpeed = 0)
		{
			if (IsVisible) return null;

			StopFadeCoroutine();

			float speed = (fadeSpeed < Mathf.Epsilon) ? gameOptions.Dialogue.FadeTransitionSpeed : fadeSpeed;
			fadeCoroutine = StartCoroutine(FadeContainer(1f, speed, FadeSpeedMultiplier));
			return fadeCoroutine;
		}

		virtual public void HideInstant()
		{
			if (IsHidden) return;

			StopFadeCoroutine();
			canvasGroup.alpha = 0f;
		}

		virtual public Coroutine Hide(float fadeSpeed = 0)
		{
			if (IsHidden) return null;

			StopFadeCoroutine();

			float speed = (fadeSpeed < Mathf.Epsilon) ? gameOptions.Dialogue.FadeTransitionSpeed : fadeSpeed;
			fadeCoroutine = StartCoroutine(FadeContainer(0f, speed, FadeSpeedMultiplier));
			return fadeCoroutine;
		}

		void StopFadeCoroutine()
		{
			if (fadeCoroutine == null) return;

			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}

		IEnumerator FadeContainer(float targetAlpha, float speed, float speedMultiplier)
		{
			float startAlpha = canvasGroup.alpha;

			float duration = (1f / speed) * speedMultiplier * (Mathf.Abs(targetAlpha - startAlpha));
			float progress = 0f;

			while (progress < duration)
			{
				progress += Time.deltaTime;
				float smoothProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / duration));
				canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothProgress);
				yield return null;
			}

			canvasGroup.alpha = targetAlpha;
			fadeCoroutine = null;
		}
	}
}
