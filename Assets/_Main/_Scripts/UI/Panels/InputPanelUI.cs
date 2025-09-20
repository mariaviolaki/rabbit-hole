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
		[SerializeField] TextMeshProUGUI titleText;
		[SerializeField] TMP_InputField inputField;
		[SerializeField] Button submitButton;
		[SerializeField] DialogueManager dialogueManager;

		string lastInput = "";
		bool isTransitioning = false;

		public event Action OnClose;

		override protected void Awake()
		{
			base.Awake();
		}

		void OnEnable()
		{
			StartCoroutine(PrepareOpen());
		}

		void OnDisable()
		{
			CompleteClose();
		}

		public IEnumerator Open(string title, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible || isTransitioning) yield break;
			isTransitioning = true;

			base.fadeSpeed = fadeSpeed;
			base.isImmediate = isImmediate;

			titleText.text = title;
			yield return SetVisible(isImmediate, fadeSpeed);

			isTransitioning = false;
		}

		public IEnumerator Close(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (isTransitioning) yield break;
			isTransitioning = true;

			fadeSpeed = fadeSpeed <= 0 ? vnOptions.General.SkipTransitionSpeed : fadeSpeed;
			yield return SetHidden(isImmediate, fadeSpeed);

			isTransitioning = false;
			OnClose?.Invoke();
		}

		void SubmitInput()
		{
			if (isTransitioning || string.IsNullOrWhiteSpace(inputField.text)) return;

			lastInput = inputField.text.Trim();
			inputManager.TriggerSubmitInput(lastInput);

			StartCoroutine(Close(isImmediate));
		}

		void SetButtonVisibility(string input)
		{
			bool isValidInput = !string.IsNullOrWhiteSpace(input);

			if (isValidInput && !submitButton.interactable)
				submitButton.interactable = true;
			else if (!isValidInput && submitButton.interactable)
				submitButton.interactable = false;
		}

		IEnumerator PrepareOpen()
		{
			yield return null;

			inputField.text = string.Empty;
			inputField.Select();
			inputField.ActivateInputField();

			if (submitButton.interactable)
				submitButton.interactable = false;

			SubscribeListeners();

			inputManager.IsInputPanelOpen = true;
			inputManager.TriggerClearInput();
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
			inputManager.OnConfirmInput += SubmitInput;
		}

		void UnsubscribeListeners()
		{
			inputField.onValueChanged.RemoveListener(SetButtonVisibility);
			submitButton.onClick.RemoveListener(SubmitInput);
			inputManager.OnConfirmInput -= SubmitInput;
		}
	}
}
