using Dialogue;
using IO;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class InputPanelUI : FadeableUI
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] DialogueManager dialogueManager;
		[SerializeField] TextMeshProUGUI titleText;
		[SerializeField] TMP_InputField inputField;
		[SerializeField] Button submitButton;

		string lastInput = "";
		bool isTransitioning = false;

		public event Action OnClose;

		override protected void Awake()
		{
			base.Awake();
		}

		override protected void OnEnable()
		{
			base.OnEnable();
			PrepareOpen();
		}

		override protected void OnDisable()
		{
			base.OnDisable();
			CompleteClose();
		}

		public IEnumerator Open(string title, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible || isTransitioning) yield break;
			isTransitioning = true;

			base.fadeSpeed = fadeSpeed;
			base.isImmediateTransition = isImmediate;

			titleText.text = title;
			yield return SetVisible(isImmediate, fadeSpeed);

			isTransitioning = false;
		}

		public IEnumerator Close(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (isTransitioning) yield break;
			isTransitioning = true;

			fadeSpeed = fadeSpeed <= 0 ? gameOptions.General.SkipTransitionSpeed : fadeSpeed;
			yield return SetHidden(isImmediate, fadeSpeed);

			isTransitioning = false;
			OnClose?.Invoke();
		}

		void SubmitInput()
		{
			if (isTransitioning || string.IsNullOrWhiteSpace(inputField.text)) return;

			lastInput = inputField.text.Trim();
			inputManager.OnSubmitInput?.Invoke(lastInput);

			StartCoroutine(Close(isImmediateTransition));
		}

		void SetButtonVisibility(string input)
		{
			bool isValidInput = !string.IsNullOrWhiteSpace(input);
			bool isActive = submitButton.gameObject.activeSelf;

			if (isValidInput && !isActive)
				submitButton.gameObject.SetActive(true);
			else if (!isValidInput && isActive)
				submitButton.gameObject.SetActive(false);
		}

		void PrepareOpen()
		{
			inputField.text = string.Empty;
			inputField.Select();
			inputField.ActivateInputField();

			if (submitButton.gameObject.activeSelf)
				submitButton.gameObject.SetActive(false);
			
			SubscribeListeners();

			inputManager.IsInputPanelOpen = true;
			inputManager.OnClearInput?.Invoke();
		}

		void CompleteClose()
		{
			UnsubscribeListeners();
			inputManager.IsInputPanelOpen = false;
		}

		void SubscribeListeners()
		{
			inputField.onValueChanged.AddListener(SetButtonVisibility);
			submitButton.onClick.AddListener(SubmitInput);
		}

		void UnsubscribeListeners()
		{
			inputField.onValueChanged.RemoveListener(SetButtonVisibility);
			submitButton.onClick.RemoveListener(SubmitInput);
		}
	}
}
