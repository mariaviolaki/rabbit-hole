using Characters;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class VisualNovelUI : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] DialogueUI dialogueUI;
		[SerializeField] FadeableUI backgroundUI;
		[SerializeField] FadeableUI spritesUI;
		[SerializeField] FadeableUI foregroundUI;
		[SerializeField] FadeableUI gameplayControlsUI;
		[SerializeField] Canvas overlayControlsCanvas;

		const float fadeMultiplier = 0.1f;
		Coroutine fadeCoroutine;

		public TextMeshProUGUI DialogueText => dialogueUI.DialogueText;

		void Start()
		{
			ToggleOverlayControls(false);
		}

		public void ShowInstant()
		{
			ToggleVisualNovelUI(true);
		}

		public Coroutine Show(float fadeSpeed)
		{
			StopFadeCoroutine();

			fadeCoroutine = StartCoroutine(FadeVisualNovelUI(true, fadeSpeed));
			return fadeCoroutine;
		}

		public void HideInstant()
		{
			ToggleVisualNovelUI(false);
		}

		public Coroutine Hide(float fadeSpeed)
		{
			StopFadeCoroutine();

			fadeCoroutine = StartCoroutine(FadeVisualNovelUI(false, fadeSpeed));
			return fadeCoroutine;
		}

		public void ShowDialogueInstant()
		{
			dialogueUI.ShowInstant();
		}

		public Coroutine ShowDialogue(float fadeSpeed = 0)
		{
			return dialogueUI.Show(fadeSpeed);
		}

		public void HideDialogueInstant()
		{
			dialogueUI.HideInstant();
		}

		public Coroutine HideDialogue(float fadeSpeed = 0)
		{
			return dialogueUI.Hide(fadeSpeed);
		}

		public Coroutine ShowSpeaker(CharacterData characterData, float fadeSpeed = 0)
		{
			return dialogueUI.ShowSpeaker(characterData, fadeSpeed);
		}

		public Coroutine HideSpeaker(float fadeSpeed = 0)
		{
			return dialogueUI.HideSpeaker(fadeSpeed);
		}

		void StopFadeCoroutine()
		{
			if (fadeCoroutine == null) return;

			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}

		void ToggleVisualNovelUI(bool isShowing)
		{
			FadeableUI[] canvases = new[] { dialogueUI, backgroundUI, spritesUI, foregroundUI, gameplayControlsUI };

			for (int i = 0; i < canvases.Length; i++)
			{
				FadeableUI canvasUI = canvases[i];

				if (isShowing) canvasUI.ShowInstant();
				else canvasUI.HideInstant();
			}
			ToggleOverlayControls(!isShowing);
		}

		void ToggleOverlayControls(bool isActive)
		{
			overlayControlsCanvas.gameObject.SetActive(isActive);
		}

		IEnumerator FadeVisualNovelUI(bool isFadeIn, float fadeSpeed = 0)
		{
			// When showing all visual novel canvases, don't include the dialogue box
			FadeableUI[] canvases = isFadeIn
				? new[] { backgroundUI, spritesUI, foregroundUI, gameplayControlsUI }
				: new[] { dialogueUI, backgroundUI, spritesUI, foregroundUI, gameplayControlsUI };

			float speed = fadeSpeed < Mathf.Epsilon ? gameOptions.Dialogue.SceneFadeTransitionSpeed : fadeSpeed;
			speed *= fadeMultiplier;

			int fadedCount = 0;

			IEnumerator MarkFadeCompletion(int canvasIndex)
			{
				FadeableUI canvasUI = canvases[canvasIndex];
				yield return isFadeIn ? canvasUI.Show(fadeSpeed) : canvasUI.Hide(fadeSpeed);
				fadedCount++;
			}

			for (int i = 0; i < canvases.Length; i++)
			{
				StartCoroutine(MarkFadeCompletion(i));
			}

			yield return new WaitUntil(() => fadedCount == canvases.Length);
			fadeCoroutine = null;
			ToggleOverlayControls(!isFadeIn);
		}
	}
}
