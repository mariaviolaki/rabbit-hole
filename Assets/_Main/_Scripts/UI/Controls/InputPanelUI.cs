using Dialogue;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Variables;

namespace UI
{
	public class InputPanelUI : BaseFadeableUI
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] DialogueSystem dialogueSystem;
		[SerializeField] TextMeshProUGUI titleText;
		[SerializeField] TMP_InputField inputField;
		[SerializeField] Button submitButton;

		ScriptTagManager scriptTagManager;
		const string InputTagName = "input";
		string lastInput = "";
		Coroutine visibilityCoroutine;

		public event Action OnClose;

		override protected void Awake()
		{
			base.Awake();
			scriptTagManager = dialogueSystem.TagManager;
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

		public Coroutine Show(string title, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible) return null;
			
			base.fadeSpeed = fadeSpeed;
			base.isImmediateTransition = isImmediate;
			StopProcess();

			visibilityCoroutine = StartCoroutine(ShowProcess(title, isImmediate, fadeSpeed));
			return visibilityCoroutine;
		}

		public Coroutine ForceHide(bool isImmediate = false)
		{
			fadeSpeed = gameOptions.General.SkipTransitionSpeed;

			StopProcess();
			visibilityCoroutine = StartCoroutine(HideProcess(isImmediate, fadeSpeed));
			return visibilityCoroutine;
		}

		public IEnumerator OpenProcess(string title, bool isImmediate = false, float speed = 0)
		{
			titleText.text = title;
			yield return FadeIn(isImmediate, speed);
		}

		public IEnumerator CloseProcess(bool isImmediate = false, float speed = 0)
		{
			yield return FadeOut(isImmediate, speed);
			OnClose?.Invoke();
		}

		void SubmitInput()
		{
			if (string.IsNullOrWhiteSpace(inputField.text)) return;

			lastInput = inputField.text.Trim();
			scriptTagManager.SetTagValue(InputTagName, lastInput);

			inputManager.OnSubmitInput?.Invoke(lastInput);

			StopProcess();
			visibilityCoroutine = StartCoroutine(HideProcess(isImmediateTransition, fadeSpeed));
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

		IEnumerator ShowProcess(string title, bool isImmediate = false, float speed = 0)
		{
			yield return OpenProcess(title, isImmediate, speed);
			visibilityCoroutine = null;
		}

		IEnumerator HideProcess(bool isImmediate = false, float speed = 0)
		{
			yield return CloseProcess(isImmediate, speed);
			visibilityCoroutine = null;
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

		void StopProcess()
		{
			if (visibilityCoroutine == null) return;

			StopCoroutine(visibilityCoroutine);
			visibilityCoroutine = null;
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
