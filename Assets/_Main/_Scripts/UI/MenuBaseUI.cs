using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class MenuBaseUI : FadeableUI
	{
		[SerializeField] Button backButton;

		MenusUI menus;
		protected bool isTransitioning = false;

		virtual public event Action OnClose;

		public bool IsTransitioning => isTransitioning;

		protected override void Awake()
		{
			menus = FindObjectOfType<MenusUI>();
			menus.SetMenu(this);
			base.Awake();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			menus.RemoveMenu(this);
		}

		override protected void OnEnable()
		{
			base.OnEnable();
			SubscribeListeners();
		}

		override protected void OnDisable()
		{
			base.OnDisable();
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
