using System.Collections;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class FadeableUI : BaseFadeableUI
	{
		Coroutine fadeCoroutine;

		public Coroutine Show(bool isImmediate = false, float fadeSpeed = 0)
		{
			StopFadeCoroutine();

			fadeCoroutine = StartCoroutine(ShowProcess(isImmediate, fadeSpeed));
			return fadeCoroutine;
		}

		public Coroutine Hide(bool isImmediate = false, float fadeSpeed = 0)
		{
			StopFadeCoroutine();

			fadeCoroutine = StartCoroutine(HideProcess(isImmediate, fadeSpeed));
			return fadeCoroutine;
		}

		IEnumerator ShowProcess(bool isImmediate, float fadeSpeed)
		{
			yield return FadeIn(isImmediate, fadeSpeed);
			fadeCoroutine = null;
		}

		IEnumerator HideProcess(bool isImmediate, float fadeSpeed)
		{
			yield return FadeOut(isImmediate, fadeSpeed);
			fadeCoroutine = null;
		}

		void StopFadeCoroutine()
		{
			if (fadeCoroutine == null) return;

			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}
	}
}
