using Dialogue;
using TMPro;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ReadModeIndicatorUI : BaseFadeableUI
	{
		[SerializeField] TextMeshProUGUI autoReadText;

		public Coroutine Hide(bool isImmediate = false, float fadeSpeed = 0f)
		{
			if (IsHidden) return null;

			return SetHidden(isImmediate, fadeSpeed);
		}

		public Coroutine Show(DialogueReadMode readMode, bool isImmediate = false, float fadeSpeed = 0f)
		{
			string newText = readMode.ToString();

			if (autoReadText.text != newText)
				autoReadText.text = newText;

			return IsVisible ? null : SetVisible(isImmediate, fadeSpeed);
		}
	}
}
