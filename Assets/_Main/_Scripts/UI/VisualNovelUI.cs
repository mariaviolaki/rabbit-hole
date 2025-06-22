using Characters;
using Logic;
using System.Collections;
using System.Collections.Generic;
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

		public DialogueUI Dialogue => dialogue;
		public InputPanelUI InputPanel => gameplayControls.InputPanel;
		public ChoicePanelUI ChoicePanel => gameplayControls.ChoicePanel;

		void Start()
		{
			ToggleOverlayControls(false);
		}

		public Coroutine Show(bool isImmediate = false, float fadeSpeed = 0)
		{
			StopFadeCoroutine();

			if (isImmediate)
			{
				ToggleVisualNovelUI(true);
				return null;
			}
			else
			{
				fadeSpeed = fadeSpeed <= Mathf.Epsilon ? gameOptions.General.TransitionSpeed : fadeSpeed;
				fadeCoroutine = StartCoroutine(FadeVisualNovelUI(true, fadeSpeed));
				return fadeCoroutine;
			}
		}

		public Coroutine Hide(bool isImmediate = false, float fadeSpeed = 0)
		{
			StopFadeCoroutine();

			if (isImmediate)
			{
				ToggleVisualNovelUI(false);
				return null;
			}
			else
			{
				fadeSpeed = fadeSpeed <= Mathf.Epsilon ? gameOptions.General.TransitionSpeed : fadeSpeed;
				fadeCoroutine = StartCoroutine(FadeVisualNovelUI(false, fadeSpeed));
				return fadeCoroutine;
			}
		}

		public Coroutine ShowSpeaker(CharacterData characterData, bool isImmediate = false, float fadeSpeed = 0)
			=> dialogue.ShowSpeaker(characterData, isImmediate, fadeSpeed);
		public Coroutine HideSpeaker(bool isImmediate = false, float fadeSpeed = 0)
			=> dialogue.HideSpeaker(isImmediate, fadeSpeed);

		public Coroutine ShowDialogue(bool isImmediate = false, float fadeSpeed = 0)
			=> dialogue.Show(isImmediate, fadeSpeed);
		public Coroutine HideDialogue(bool isImmediate = false, float fadeSpeed = 0)
			=> dialogue.Hide(isImmediate, fadeSpeed);

		public Coroutine ShowInput(string title, bool isImmediate = false, float fadeSpeed = 0)
			=> gameplayControls.ShowInput(title, isImmediate, fadeSpeed);

		public Coroutine ShowChoices(List<DialogueChoice> choices, bool isImmediate = false, float fadeSpeed = 0)
			=> gameplayControls.ShowChoices(choices, isImmediate, fadeSpeed);

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

				if (isShowing) canvasUI.Show(true);
				else canvasUI.Hide(true);
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
				yield return isFadeIn ? canvasUI.Show(false, fadeSpeed) : canvasUI.Hide(false, fadeSpeed);
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
