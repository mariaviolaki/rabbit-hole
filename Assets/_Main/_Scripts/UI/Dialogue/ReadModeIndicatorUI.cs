using Dialogue;
using TMPro;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ReadModeIndicatorUI : BaseFadeableUI
	{
		[SerializeField] TextMeshProUGUI autoReadText;
		Coroutine visibilityCoroutine;

		public Coroutine Hide(bool isImmediate = false, float fadeSpeed = 0f)
		{
			if (IsHidden) return null;

			StopProcess();
			visibilityCoroutine = StartCoroutine(FadeOut(isImmediate, fadeSpeed));
			return visibilityCoroutine;
		}

		public Coroutine Show(DialogueReadMode readMode, bool isImmediate = false, float fadeSpeed = 0f)
		{
			string newText = readMode.ToString();

			if (autoReadText.text != newText)
				autoReadText.text = newText;

			if (IsVisible) return null;

			StopProcess();
			visibilityCoroutine = StartCoroutine(FadeIn(isImmediate, fadeSpeed));
			return visibilityCoroutine;
		}

		void StopProcess()
		{
			if (visibilityCoroutine == null) return;

			StopCoroutine(visibilityCoroutine);
			visibilityCoroutine = null;
		}
	}
}
