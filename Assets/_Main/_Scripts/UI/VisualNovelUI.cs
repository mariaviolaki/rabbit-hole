using Characters;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
	public class VisualNovelUI : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] DialogueUI dialogue;
		[SerializeField] FadeableUI background;
		[SerializeField] FadeableUI sprites;
		[SerializeField] FadeableUI foreground;
		[SerializeField] GameplayControlsUI gameplayControls;
		[SerializeField] Canvas overlayControlsCanvas;

		const float fadeMultiplier = 0.1f;
		Coroutine fadeCoroutine;

		public TextMeshProUGUI DialogueText => dialogue.DialogueText;

		void Start()
		{
			ToggleOverlayControls(false);
		}

		public void ShowInstant()
		{
			ToggleVisualNovelUI(true);
		}

		public Coroutine Show(float fadeSpeed = 0)
		{
			StopFadeCoroutine();

			fadeSpeed = fadeSpeed <= Mathf.Epsilon ? gameOptions.General.TransitionSpeed : fadeSpeed;
			fadeCoroutine = StartCoroutine(FadeVisualNovelUI(true, fadeSpeed));
			return fadeCoroutine;
		}

		public void HideInstant()
		{
			ToggleVisualNovelUI(false);
		}

		public Coroutine Hide(float fadeSpeed = 0)
		{
			StopFadeCoroutine();

			fadeSpeed = fadeSpeed <= Mathf.Epsilon ? gameOptions.General.TransitionSpeed : fadeSpeed;
			fadeCoroutine = StartCoroutine(FadeVisualNovelUI(false, fadeSpeed));
			return fadeCoroutine;
		}

		public Coroutine ShowSpeaker(CharacterData characterData, float fadeSpeed = 0) => dialogue.ShowSpeaker(characterData, fadeSpeed);
		public Coroutine HideSpeaker(float fadeSpeed = 0) => dialogue.HideSpeaker(fadeSpeed);

		public void ShowDialogueInstant() => dialogue.ShowInstant();
		public Coroutine ShowDialogue(float fadeSpeed = 0) => dialogue.Show(fadeSpeed);
		public void HideDialogueInstant() => dialogue.HideInstant();
		public Coroutine HideDialogue(float fadeSpeed = 0) => dialogue.Hide(fadeSpeed);

		public void ShowInputInstant(string title) => gameplayControls.ShowInputInstant(title);
		public Coroutine ShowInput(string title, float fadeSpeed = 0) => gameplayControls.ShowInput(title, fadeSpeed);

		void StopFadeCoroutine()
		{
			if (fadeCoroutine == null) return;

			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}

		void ToggleVisualNovelUI(bool isShowing)
		{
			FadeableUI[] canvases = new[] { dialogue, background, sprites, foreground, gameplayControls };

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
				? new[] { background, sprites, foreground, gameplayControls }
				: new[] { dialogue, background, sprites, foreground, gameplayControls };

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

			while (fadedCount != canvases.Length) yield return null;
			fadeCoroutine = null;
			ToggleOverlayControls(!isFadeIn);
		}
	}
}
