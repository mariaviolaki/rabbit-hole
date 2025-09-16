using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class MenuBaseUI : FadeableUI
	{
		[SerializeField] Button backButton;
		[SerializeField] protected MenusUI menus;

		protected bool isTransitioning = false;

		public Action OnClose;

		public bool IsTransitioning => isTransitioning;

		virtual protected void Start()
		{
			SubscribeListeners();
			OnClose?.Invoke();
		}

		virtual protected void OnDestroy()
		{
			UnsubscribeListeners();
		}

		public IEnumerator Open(MenuType menuType, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible || isTransitioning) yield break;
			isTransitioning = true;

			base.fadeSpeed = fadeSpeed;
			isImmediateTransition = isImmediate;

			if (!PrepareOpen(menuType))
			{
				isTransitioning = false;
				OnClose?.Invoke();
				yield break;
			}

			yield return SetVisible(isImmediate, fadeSpeed);

			isTransitioning = false;
		}

		public IEnumerator Close(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsHidden || isTransitioning) yield break;
			isTransitioning = true;

			CompleteClose();

			fadeSpeed = fadeSpeed <= 0 ? vnOptions.General.TransitionSpeed : fadeSpeed;
			yield return SetHidden(isImmediate, fadeSpeed);

			isTransitioning = false;
			OnClose?.Invoke();
		}

		virtual protected bool PrepareOpen(MenuType menuType) { return true; }

		virtual protected void CompleteClose() { }

		virtual protected void SubscribeListeners()
		{
			backButton.onClick.AddListener(menus.CloseMenu);
		}

		virtual protected void UnsubscribeListeners()
		{
			backButton.onClick.RemoveListener(menus.CloseMenu);
		}
	}
}
