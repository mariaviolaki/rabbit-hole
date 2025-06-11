using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
	public class TransitionUtils
	{
		const float FadeSpeedMultiplier = 5f;
		const float ColorSpeedMultiplier = 5f;

		public static IEnumerator SetNewImage(CanvasGroup oldCanvasGroup, CanvasGroup newCanvasGroup, float speed)
		{
			float duration = (1 / speed) * FadeSpeedMultiplier;

			float oldCanvasGroupStartAlpha = oldCanvasGroup.alpha;
			float newCanvasGroupStartAlpha = newCanvasGroup.alpha;

			float progress = 0f;
			while (progress < duration)
			{
				progress += Time.deltaTime;
				float smoothProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / duration));

				oldCanvasGroup.alpha = Mathf.Lerp(oldCanvasGroupStartAlpha, 0f, smoothProgress);
				newCanvasGroup.alpha = Mathf.Lerp(newCanvasGroupStartAlpha, 1f, smoothProgress);

				yield return null;
			}

			newCanvasGroup.alpha = 1f;
			oldCanvasGroup.alpha = 0f;
		}

		public static IEnumerator SetImageVisibility(CanvasGroup canvasGroup, bool isVisible, float speed)
		{
			float startAlpha = canvasGroup.alpha;
			float targetAlpha = isVisible ? 1f : 0f;

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

		public static IEnumerator SetImageColor(Graphic image, Color targetColor, float speed)
		{
			Color startColor = image.color;
			float duration = (1 / speed) * ColorSpeedMultiplier * Vector4.Distance(startColor, targetColor);

			float timeElapsed = 0;
			while (timeElapsed < duration)
			{
				timeElapsed += Time.deltaTime;
				float smoothPercentage = Mathf.SmoothStep(0, 1, Mathf.Clamp01(timeElapsed / duration));

				image.color = Color.Lerp(startColor, targetColor, smoothPercentage);
				yield return null;
			}

			image.color = targetColor;
		}
	}
}

