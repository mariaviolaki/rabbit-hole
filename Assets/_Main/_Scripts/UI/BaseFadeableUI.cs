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

			SetHidden(true);
		}

		protected Coroutine SetVisible(bool isImmediate = false, float speed = 0)
		{
			if (IsVisible) return null;

			StopFadeCoroutine();

			if (isImmediate)
			{
				canvasGroup.alpha = 1f;
				return null;
			}
			else
			{
				fadeSpeed = (speed < Mathf.Epsilon) ? gameOptions.Dialogue.FadeTransitionSpeed : speed;
				fadeCoroutine = StartCoroutine(transitionHandler.SetVisibility(canvasGroup, true, fadeSpeed));
				return fadeCoroutine;
			}
		}

		protected Coroutine SetHidden(bool isImmediate = false, float speed = 0)
		{
			if (IsHidden) return null;

			StopFadeCoroutine();

			if (isImmediate)
			{
				canvasGroup.alpha = 0f;
				return null;
			}
			else
			{
				fadeSpeed = (speed < Mathf.Epsilon) ? gameOptions.Dialogue.FadeTransitionSpeed : speed;
				fadeCoroutine = StartCoroutine(transitionHandler.SetVisibility(canvasGroup, false, fadeSpeed));
				return fadeCoroutine;
			}
		}

		void StopFadeCoroutine()
		{
			if (fadeCoroutine == null) return;

			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}
	}
}
