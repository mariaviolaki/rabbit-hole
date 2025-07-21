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

		protected float fadeSpeed;
		protected bool isImmediateTransition;

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

			SetHiddenImmediate();
		}

		public void SetVisibleImmediate() => canvasGroup.alpha = 1f;
		public void SetHiddenImmediate() => canvasGroup.alpha = 0f;

		public virtual IEnumerator FadeIn(bool isImmediate = false, float speed = 0)
		{
			if (IsVisible) yield break;

			if (isImmediate)
			{
				SetVisibleImmediate();
				yield break;
			}
			else
			{
				fadeSpeed = (speed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : speed;
				yield return transitionHandler.SetVisibility(canvasGroup, true, fadeSpeed);
			}
		}

		public virtual IEnumerator FadeOut(bool isImmediate = false, float speed = 0)
		{
			if (IsHidden) yield break;

			if (isImmediate)
			{
				SetHiddenImmediate();
				yield break;
			}
			else
			{
				fadeSpeed = (speed < Mathf.Epsilon) ? gameOptions.General.TransitionSpeed : speed;
				yield return transitionHandler.SetVisibility(canvasGroup, false, fadeSpeed);
			}
		}
	}
}
