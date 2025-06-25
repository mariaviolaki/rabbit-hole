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
		bool isImmediate = false;
		string lastInput = "";

		public event Action OnClose;

		override protected void Awake()
		{
			base.Awake();
			scriptTagManager = dialogueSystem.TagManager;
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

		public Coroutine Show(string title, bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible) return null;

			base.fadeSpeed = fadeSpeed;
			this.isImmediate = isImmediate;
			PrepareInputPanel(title);

			return SetVisible(isImmediate, fadeSpeed);
		}

		public Coroutine ForceHide(bool isImmediate = false)
		{
			this.isImmediate = isImmediate;
			fadeSpeed = gameOptions.General.SkipTransitionSpeed;
			return StartCoroutine(HideAndClose());
		}

		void SubmitInput()
		{
			if (string.IsNullOrWhiteSpace(inputField.text)) return;

			lastInput = inputField.text.Trim();
			scriptTagManager.SetTagValue(InputTagName, lastInput);

			inputManager.OnSubmitInput?.Invoke(lastInput);
			StartCoroutine(HideAndClose());
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

		void PrepareInputPanel(string title)
		{
			titleText.text = title;
			inputField.text = string.Empty;
			inputField.Select();
			inputField.ActivateInputField();

			if (submitButton.gameObject.activeSelf)
				submitButton.gameObject.SetActive(false);

			inputManager.IsInputPanelOpen = true;
			inputManager.OnClearInput?.Invoke();
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

		IEnumerator HideAndClose()
		{
			inputField.text = string.Empty;
			inputManager.IsInputPanelOpen = false;
			yield return SetHidden(isImmediate, fadeSpeed);
			OnClose?.Invoke();
		}
	}
}
