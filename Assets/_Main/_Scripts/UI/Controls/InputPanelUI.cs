using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class InputPanelUI : BaseFadeableUI
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] TextMeshProUGUI titleText;
		[SerializeField] TMP_InputField inputField;
		[SerializeField] Button submitButton;

		bool isImmediate = false;
		string lastInput = "";

		public string LastInput => lastInput;

		public event Action OnClose;

		override protected void OnEnable()
		{
			base.OnEnable();
			SubscribeListeners();
		}

		protected override void OnDisable()
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

		Coroutine Hide()
		{
			if (IsHidden) return null;

			return StartCoroutine(HideAndClose());
		}

		void SubmitInput()
		{
			if (string.IsNullOrWhiteSpace(inputField.text)) return;

			lastInput = inputField.text.Trim();

			inputManager.IsInputPanelOpen = false;
			inputManager.OnSubmitInput?.Invoke(lastInput);
			Hide();
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
			yield return SetHidden(isImmediate, fadeSpeed);
			OnClose?.Invoke();
		}
	}
}
