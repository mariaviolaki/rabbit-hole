using Characters;
using Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class VisualNovelUI : MonoBehaviour
	{
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] InputManagerSO inputManager;
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

			inputManager.IsChoicePanelOpen = false;
			inputManager.IsInputPanelOpen = false;
		}

		public Coroutine ShowSpeaker(CharacterData characterData, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (fadeCoroutine != null) return null;
			return dialogue.ShowSpeaker(characterData, isImmediate, fadeSpeed);
		}
		public Coroutine HideSpeaker(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (fadeCoroutine != null) return null;
			return dialogue.HideSpeaker(isImmediate, fadeSpeed);
		}

		public Coroutine ShowDialogue(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (fadeCoroutine != null) return null;
			return dialogue.Show(isImmediate, fadeSpeed);
		}
		public Coroutine HideDialogue(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (fadeCoroutine != null) return null;
			return dialogue.Hide(isImmediate, fadeSpeed);
		}

		public Coroutine ShowInput(string title, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (fadeCoroutine != null) return null;
			return gameplayControls.ShowInput(title, isImmediate, fadeSpeed);
		}
		public Coroutine ForceHideInput(bool isImmediate = false)
		{
			if (fadeCoroutine != null) return null;
			return gameplayControls.ForceHideInput(isImmediate);
		}

		public Coroutine ShowChoices(List<DialogueChoice> choices, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (fadeCoroutine != null) return null;
			return gameplayControls.ShowChoices(choices, isImmediate, fadeSpeed);
		}
		public Coroutine ForceHideChoices(bool isImmediate = false)
		{
			if (fadeCoroutine != null) return null;
			return gameplayControls.ForceHideChoices(isImmediate);
		}

		public Coroutine Show(bool isImmediate = false, float fadeSpeed = 0) => SetVisiblity(true, isImmediate, fadeSpeed);
		public Coroutine Hide(bool isImmediate = false, float fadeSpeed = 0) => SetVisiblity(false, isImmediate, fadeSpeed);
		Coroutine SetVisiblity(bool isVisible, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (fadeCoroutine != null) return null;

			if (isImmediate)
			{
				ToggleVisualNovelUI(isVisible);
				return null;
			}
			else
			{
				fadeSpeed = fadeSpeed <= Mathf.Epsilon ? gameOptions.General.TransitionSpeed : fadeSpeed;
				fadeCoroutine = StartCoroutine(FadeVisualNovelUI(isVisible, fadeSpeed));
				return fadeCoroutine;
			}
		}

		void ToggleVisualNovelUI(bool isShowing)
		{
			BaseFadeableUI[] canvases = new BaseFadeableUI[] { dialogue, background, sprites, foreground, gameplayControls };

			for (int i = 0; i < canvases.Length; i++)
			{
				BaseFadeableUI canvasUI = canvases[i];

				if (isShowing) canvasUI.SetVisibleImmediate();
				else canvasUI.SetHiddenImmediate();
			}

			ToggleOverlayControls(!isShowing);
		}

		IEnumerator FadeVisualNovelUI(bool isFadeIn, float fadeSpeed = 0)
		{
			// When showing all visual novel canvases, don't include the dialogue box
			BaseFadeableUI[] canvases = isFadeIn
				? new BaseFadeableUI[] { background, sprites, foreground, gameplayControls }
				: new BaseFadeableUI[] { dialogue, background, sprites, foreground, gameplayControls };

			float speed = fadeSpeed < Mathf.Epsilon ? gameOptions.Dialogue.SceneFadeTransitionSpeed : fadeSpeed;
			speed *= fadeMultiplier;

			List<IEnumerator> fadeProcesses = new();
			foreach (BaseFadeableUI canvas in canvases)
			{
				if (isFadeIn)
					fadeProcesses.Add(canvas.FadeIn(false, speed));
				else
					fadeProcesses.Add(canvas.FadeOut(false, speed));
			}

			yield return Utilities.RunConcurrentProcesses(fadeProcesses);
			
			ToggleOverlayControls(!isFadeIn);
			fadeCoroutine = null;
		}

		void ToggleOverlayControls(bool isActive)
		{
			overlayControlsCanvas.gameObject.SetActive(isActive);
		}
	}
}
