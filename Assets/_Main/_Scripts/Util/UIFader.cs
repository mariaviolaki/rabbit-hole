using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITransitionHandler
{
	const float MinTransitionDuration = 0.001f;
	const float MaxTransitionDuration = 10f;
	const float FadeSpeedMultiplier = 5f;
	const float ColorSpeedMultiplier = 5f;
	readonly float DefaultTransitionSpeed;

	public UITransitionHandler(GameOptionsSO gameOptions)
	{
		DefaultTransitionSpeed = gameOptions.General.TransitionSpeed;
	}

	public IEnumerator Replace(CanvasGroup oldCanvasGroup, CanvasGroup newCanvasGroup, float speed = 0)
	{
		if (oldCanvasGroup == null || newCanvasGroup == null) yield break;

		speed = speed <= Mathf.Epsilon ? DefaultTransitionSpeed : speed;

		float oldCanvasGroupStartAlpha = oldCanvasGroup.alpha;
		float newCanvasGroupStartAlpha = newCanvasGroup.alpha;

		float duration = (1 / speed) * FadeSpeedMultiplier;
		duration = Mathf.Clamp(duration, MinTransitionDuration, MaxTransitionDuration);

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

	public IEnumerator SetVisibility(CanvasGroup canvasGroup, bool isVisible, float speed = 0)
	{
		if (canvasGroup == null) yield break;

		speed = speed <= Mathf.Epsilon ? DefaultTransitionSpeed : speed;

		float startAlpha = canvasGroup.alpha;
		float targetAlpha = isVisible ? 1f : 0f;

		float duration = (1f / speed) * FadeSpeedMultiplier * Mathf.Abs(targetAlpha - startAlpha);
		duration = Mathf.Clamp(duration, MinTransitionDuration, MaxTransitionDuration);

		float progress = 0;
		while (progress < duration)
		{
			progress += Time.deltaTime;
			float smoothPercentage = Mathf.SmoothStep(0, 1, Mathf.Clamp01(progress / duration));

			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothPercentage);
			yield return null;
		}

		canvasGroup.alpha = targetAlpha;
	}

	public IEnumerator SetColor(Graphic image, Color startColor, Color targetColor, float speed = 0)
	{
		if (image == null) yield break;

		speed = speed <= Mathf.Epsilon ? DefaultTransitionSpeed : speed;

		float duration = (1 / speed) * ColorSpeedMultiplier * Vector4.Distance(startColor, targetColor);
		duration = Mathf.Clamp(duration, MinTransitionDuration, MaxTransitionDuration);

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
