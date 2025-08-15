using IO;
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
		[SerializeField] DialogueContinuePromptUI continuePrompt;
		[SerializeField] ReadModeIndicatorUI readModeIndicator;
		[SerializeField] Canvas overlayControlsCanvas;

		const float fadeMultiplier = 0.1f;

		public DialogueUI Dialogue => dialogue;
		public GameplayControlsUI GameplayControls => gameplayControls;
		public DialogueContinuePromptUI ContinuePrompt => continuePrompt;
		public ReadModeIndicatorUI ReadModeIndicator => readModeIndicator;

		void Start()
		{
			ToggleOverlayControls(false);
		}

		public IEnumerator Show(bool isImmediate = false, float fadeSpeed = 0) => SetVisiblity(true, isImmediate, fadeSpeed);
		public IEnumerator Hide(bool isImmediate = false, float fadeSpeed = 0) => SetVisiblity(false, isImmediate, fadeSpeed);
		IEnumerator SetVisiblity(bool isVisible, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (isImmediate)
			{
				ToggleVisualNovelUI(isVisible);
				yield break;
			}

			fadeSpeed = fadeSpeed <= Mathf.Epsilon ? gameOptions.General.TransitionSpeed : fadeSpeed;
			yield return FadeVisualNovelUI(isVisible, fadeSpeed);
		}

		void ToggleVisualNovelUI(bool isShowing)
		{
			FadeableUI[] canvases = new FadeableUI[] { dialogue, background, sprites, foreground, gameplayControls };

			for (int i = 0; i < canvases.Length; i++)
			{
				FadeableUI canvasUI = canvases[i];

				if (isShowing) canvasUI.SetVisibleImmediate();
				else canvasUI.SetHiddenImmediate();
			}

			ToggleOverlayControls(!isShowing);
		}

		IEnumerator FadeVisualNovelUI(bool isFadeIn, float fadeSpeed = 0)
		{
			// When showing all visual novel canvases, don't include the dialogue box
			FadeableUI[] canvases = isFadeIn
				? new FadeableUI[] { background, sprites, foreground, gameplayControls }
				: new FadeableUI[] { dialogue, background, sprites, foreground, gameplayControls };

			float speed = fadeSpeed < Mathf.Epsilon ? gameOptions.General.SceneFadeTransitionSpeed : fadeSpeed;
			speed *= fadeMultiplier;

			List<IEnumerator> fadeProcesses = new();
			foreach (FadeableUI canvas in canvases)
			{
				if (!canvas.gameObject.activeInHierarchy) continue;

				if (isFadeIn)
					fadeProcesses.Add(canvas.SetVisible(false, speed));
				else
					fadeProcesses.Add(canvas.SetHidden(false, speed));
			}

			if (fadeProcesses.Count > 0)
				yield return Utilities.RunConcurrentProcesses(this, fadeProcesses);
			
			ToggleOverlayControls(!isFadeIn);
		}

		void ToggleOverlayControls(bool isActive)
		{
			overlayControlsCanvas.gameObject.SetActive(isActive);
		}
	}
}
