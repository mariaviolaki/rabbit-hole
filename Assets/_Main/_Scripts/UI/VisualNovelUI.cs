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
		[SerializeField] Canvas overlayControlsCanvas;

		const float fadeMultiplier = 0.1f;
		Coroutine fadeCoroutine;

		public DialogueUI Dialogue => dialogue;
		public GameplayControlsUI GameplayControls => gameplayControls;

		void Start()
		{
			ToggleOverlayControls(false);
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

			float speed = fadeSpeed < Mathf.Epsilon ? gameOptions.General.SceneFadeTransitionSpeed : fadeSpeed;
			speed *= fadeMultiplier;

			List<IEnumerator> fadeProcesses = new();
			foreach (BaseFadeableUI canvas in canvases)
			{
				if (isFadeIn)
					fadeProcesses.Add(canvas.FadeIn(false, speed));
				else
					fadeProcesses.Add(canvas.FadeOut(false, speed));
			}

			yield return Utilities.RunConcurrentProcesses(this, fadeProcesses);
			
			ToggleOverlayControls(!isFadeIn);
			fadeCoroutine = null;
		}

		void ToggleOverlayControls(bool isActive)
		{
			overlayControlsCanvas.gameObject.SetActive(isActive);
		}
	}
}
