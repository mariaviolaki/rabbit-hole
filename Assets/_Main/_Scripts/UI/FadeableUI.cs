using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class FadeableUI : BaseFadeableUI
	{
		public Coroutine Show(bool isImmediate = false, float fadeSpeed = 0) => SetVisible(isImmediate, fadeSpeed);
		public Coroutine Hide(bool isImmediate = false, float fadeSpeed = 0) => SetHidden(isImmediate, fadeSpeed);
	}
}
