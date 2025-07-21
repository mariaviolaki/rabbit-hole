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

		public void Hide(bool isImmediate = false, float fadeSpeed = 0f)
		{
			if (IsHidden) return;

			StopProcess();
			visibilityCoroutine = StartCoroutine(FadeOut(isImmediate, fadeSpeed));
		}

		public void Show(DialogueReadMode readMode, bool isImmediate = false, float fadeSpeed = 0f)
		{
			string newText = readMode.ToString();

			if (autoReadText.text != newText)
				autoReadText.text = newText;

			if (IsVisible) return;

			StopProcess();
			visibilityCoroutine = StartCoroutine(FadeIn(isImmediate, fadeSpeed));
		}

		void StopProcess()
		{
			if (visibilityCoroutine == null) return;

			StopCoroutine(visibilityCoroutine);
			visibilityCoroutine = null;
		}
	}
}
