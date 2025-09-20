using IO;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ConfirmationMenuUI : FadeableUI
	{
		[SerializeField] TextMeshProUGUI titleText;
		[SerializeField] Button acceptButton;
		[SerializeField] Button rejectButton;
		[SerializeField] InputManagerSO inputManager;

		Action acceptAction;
		Action rejectAction;
		bool isShowing = false;
		bool isHiding = false;

		bool IsTransitioning => isShowing || isHiding;

		override protected void Awake()
		{
			base.Awake();
			CloseMenu();
		}

		public void Open(string title, Action acceptAction, Action rejectAction = null, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible || IsTransitioning) return;
			
			isShowing = true;
			this.acceptAction = acceptAction;
			this.rejectAction = rejectAction;
			titleText.text = title;

			OpenMenu();
			SubscribeListeners();
			StartCoroutine(Show(isImmediate, fadeSpeed));
		}

		void Hide()
		{
			if (IsHidden || isHiding) return;

			StartCoroutine(Hide(isImmediate, fadeSpeed));
		}

		IEnumerator Show(bool isImmediate = false, float fadeSpeed = 0)
		{
			yield return SetVisible(isImmediate, fadeSpeed);
			isShowing = false;
		}

		IEnumerator Hide(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (isHiding) yield break;
			while (isShowing) yield return null;
			isHiding = true;

			fadeSpeed = fadeSpeed <= 0 ? vnOptions.General.TransitionSpeed : fadeSpeed;
			yield return SetHidden(isImmediate, fadeSpeed);

			isHiding = false;

			UnsubscribeListeners();
			CloseMenu();
		}

		void OpenMenu()
		{
			inputManager.IsConfirmationOpen = true;
			gameObject.SetActive(true);
		}

		void CloseMenu()
		{
			acceptAction = null;
			rejectAction = null;
			inputManager.IsConfirmationOpen = false;
			gameObject.SetActive(false);
		}

		void RunAcceptAction()
		{
			acceptAction?.Invoke();
			Hide();
		}

		void RunRejectAction()
		{
			rejectAction?.Invoke();
			Hide();
		}

		void SubscribeListeners()
		{
			UnsubscribeListeners();
			inputManager.OnConfirmationAccept += RunAcceptAction;
			inputManager.OnConfirmationReject += RunRejectAction;
			acceptButton.onClick.AddListener(RunAcceptAction);
			rejectButton.onClick.AddListener(RunRejectAction);
		}

		void UnsubscribeListeners()
		{
			inputManager.OnConfirmationAccept -= RunAcceptAction;
			inputManager.OnConfirmationReject -= RunRejectAction;
			acceptButton.onClick.RemoveListener(RunAcceptAction);
			rejectButton.onClick.RemoveListener(RunRejectAction);
		}
	}
}
