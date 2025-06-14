using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class FadeableUI : BaseFadeableUI
	{
		public void ShowInstant() => SetVisible();
		public void HideInstant() => SetHidden();

		public Coroutine Show(float fadeSpeed = 0) => FadeIn(fadeSpeed);
		public Coroutine Hide(float fadeSpeed = 0) => FadeOut(fadeSpeed);
	}
}
