using Dialogue;
using TMPro;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ReadModeIndicatorUI : BaseFadeableUI
	{
		[SerializeField] TextMeshProUGUI autoReadText;

		public Coroutine Hide(bool isImmediate = false, float fadeSpeed = 0f) => SetHidden(isImmediate, fadeSpeed);
		public Coroutine Show(DialogueReadMode readMode, bool isImmediate = false, float fadeSpeed = 0f)
		{
			if (IsVisible) return null;

			autoReadText.text = readMode.ToString();
			return SetVisible(isImmediate, fadeSpeed);
		}
	}
}
