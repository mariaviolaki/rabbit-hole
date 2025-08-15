using Dialogue;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ReadModeIndicatorUI : FadeableUI
	{
		[SerializeField] TextMeshProUGUI autoReadText;

		bool isTransitioning = false;

		public IEnumerator Hide(bool isImmediate = false, float fadeSpeed = 0f)
		{
			while (isTransitioning) yield return null;
			if (IsHidden) yield break;

			isTransitioning = true;
			yield return SetHidden(isImmediate, fadeSpeed);
			isTransitioning = false;
		}

		public IEnumerator Show(DialogueReadMode readMode, bool isImmediate = false, float fadeSpeed = 0f)
		{
			string newText = readMode.ToString();

			if (autoReadText.text != newText)
				autoReadText.text = newText;

			while (isTransitioning) yield return null;
			if (IsVisible) yield break;

			isTransitioning = true;
			yield return SetVisible(isImmediate, fadeSpeed);
			isTransitioning = false;
		}
	}
}
