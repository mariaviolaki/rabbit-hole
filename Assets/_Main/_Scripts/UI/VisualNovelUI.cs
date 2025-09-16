using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class VisualNovelUI : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] DialogueUI dialogue;
		[SerializeField] FadeableUI background;
		[SerializeField] FadeableUI characters;
		[SerializeField] FadeableUI foreground;
		[SerializeField] GameplayControlsUI gameplayControls;
		[SerializeField] FadeableUI cinematics;
		[SerializeField] DialogueContinuePromptUI continuePrompt;
		[SerializeField] ReadModeIndicatorUI readModeIndicator;
		[SerializeField] Canvas overlayControlsCanvas;
		[SerializeField] ScreenshotCamera screenshotCamera;
		
		const float fadeMultiplier = 0.1f;

		public DialogueUI Dialogue => dialogue;
		public GameplayControlsUI GameplayControls => gameplayControls;
		public DialogueContinuePromptUI ContinuePrompt => continuePrompt;
		public ReadModeIndicatorUI ReadModeIndicator => readModeIndicator;

		void Start()
		{
			ToggleOverlayControls(false);

			dialogue.UICanvas.worldCamera = screenshotCamera.VNCamera;
			background.UICanvas.worldCamera = screenshotCamera.VNCamera;
			characters.UICanvas.worldCamera = screenshotCamera.VNCamera;
			foreground.UICanvas.worldCamera = screenshotCamera.VNCamera;
			cinematics.UICanvas.worldCamera = screenshotCamera.VNCamera;
			gameplayControls.UICanvas.worldCamera = screenshotCamera.VNCamera;
			overlayControlsCanvas.worldCamera = screenshotCamera.VNCamera;
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

			fadeSpeed = fadeSpeed <= Mathf.Epsilon ? vnOptions.General.TransitionSpeed : fadeSpeed;
			yield return FadeVisualNovelUI(isVisible, fadeSpeed);
		}

		void ToggleVisualNovelUI(bool isShowing)
		{
			FadeableUI[] canvases = new FadeableUI[] { dialogue, background, characters, foreground, gameplayControls, cinematics };

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
				? new FadeableUI[] { background, characters, foreground, gameplayControls, cinematics }
				: new FadeableUI[] { dialogue, background, characters, foreground, gameplayControls, cinematics };

			float speed = fadeSpeed < Mathf.Epsilon ? vnOptions.General.SceneFadeTransitionSpeed : fadeSpeed;
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
