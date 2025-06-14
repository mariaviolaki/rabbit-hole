using System.Collections;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class BaseFadeableUI : MonoBehaviour
	{
		[SerializeField] protected GameOptionsSO gameOptions;

		UITransitionHandler transitionHandler;
		CanvasGroup canvasGroup;
		Coroutine fadeCoroutine;

		protected float fadeSpeed;

		public bool IsVisible => canvasGroup.alpha == 1f;
		public bool IsHidden => canvasGroup.alpha == 0f;

		protected virtual void OnEnable() { }
		protected virtual void Start() { }
		protected virtual void OnDisable() { }
		protected virtual void OnDestroy() { }

		virtual protected void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
			transitionHandler = new UITransitionHandler(gameOptions);

			SetHidden();
		}

		protected void SetVisible()
		{
			if (IsVisible) return;

			StopFadeCoroutine();
			canvasGroup.alpha = 1f;
		}

		protected void SetHidden()
		{
			if (IsHidden) return;

			StopFadeCoroutine();
			canvasGroup.alpha = 0f;
		}

		protected Coroutine FadeIn(float speed)
		{
			if (IsVisible) return null;

			StopFadeCoroutine();

			fadeSpeed = (speed < Mathf.Epsilon) ? gameOptions.Dialogue.FadeTransitionSpeed : speed;
			fadeCoroutine = StartCoroutine(FadeInProcess());
			return fadeCoroutine;
		}

		protected Coroutine FadeOut(float speed)
		{
			if (IsHidden) return null;

			StopFadeCoroutine();

			fadeSpeed = (speed < Mathf.Epsilon) ? gameOptions.Dialogue.FadeTransitionSpeed : speed;
			fadeCoroutine = StartCoroutine(FadeOutProcess());
			return fadeCoroutine;
		}

		void StopFadeCoroutine()
		{
			if (fadeCoroutine == null) return;

			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}

		protected virtual IEnumerator FadeInProcess()
		{
			yield return transitionHandler.SetVisibility(canvasGroup, true, fadeSpeed);
		}

		protected virtual IEnumerator FadeOutProcess()
		{
			yield return transitionHandler.SetVisibility(canvasGroup, false, fadeSpeed);
		}
	}
}
