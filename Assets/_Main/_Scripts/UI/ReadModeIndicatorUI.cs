using Dialogue;
using TMPro;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ReadModeIndicatorUI : BaseFadeableUI
	{
		[SerializeField] TextMeshProUGUI autoReadText;

		public void HideInstant() => SetHidden();
		public void ShowInstant(DialogueReadMode readMode)
		{
			if (IsVisible) return;

			autoReadText.text = readMode.ToString();
			SetVisible();
		}

		public Coroutine Hide(float fadeSpeed = 0f) => FadeOut(fadeSpeed);
		public Coroutine Show(DialogueReadMode readMode, float fadeSpeed = 0f)
		{
			if (IsVisible) return null;

			autoReadText.text = readMode.ToString();
			return FadeIn(fadeSpeed);
		}
	}
}
