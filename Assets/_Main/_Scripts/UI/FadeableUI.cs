using System.Collections;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class FadeableUI : MonoBehaviour
	{
		[SerializeField] Canvas canvas;
		[SerializeField] protected VNOptionsSO vnOptions;
		[SerializeField] protected CanvasGroup canvasGroup;

		protected UITransitionHandler transitionHandler;
		protected float fadeSpeed;
		protected bool isImmediateTransition;
		public Canvas UICanvas => canvas;
		public bool IsVisible => canvasGroup.alpha == 1f;
		public bool IsHidden => canvasGroup.alpha == 0f;

		virtual protected void Awake()
		{
			transitionHandler = new UITransitionHandler(vnOptions);
			SetHiddenImmediate();
		}

		public void SetVisibleImmediate() => canvasGroup.alpha = 1f;
		public void SetHiddenImmediate() => canvasGroup.alpha = 0f;

		public virtual IEnumerator SetVisible(bool isImmediate = false, float speed = 0)
		{
			if (IsVisible) yield break;

			if (isImmediate)
			{
				SetVisibleImmediate();
				yield break;
			}
			else
			{
				fadeSpeed = (speed < Mathf.Epsilon) ? vnOptions.General.TransitionSpeed : speed;
				yield return transitionHandler.SetVisibility(canvasGroup, true, fadeSpeed);
			}
		}

		public virtual IEnumerator SetHidden(bool isImmediate = false, float speed = 0)
		{
			if (IsHidden) yield break;

			if (isImmediate)
			{
				SetHiddenImmediate();
				yield break;
			}
			else
			{
				fadeSpeed = (speed < Mathf.Epsilon) ? vnOptions.General.TransitionSpeed : speed;
				yield return transitionHandler.SetVisibility(canvasGroup, false, fadeSpeed);
			}
		}
	}
}
