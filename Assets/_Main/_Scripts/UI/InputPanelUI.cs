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
			ResetInput(title);

			return SetVisible(isImmediate, fadeSpeed);
		}

		Coroutine Hide()
		{
			if (IsHidden) return null;

			return SetHidden(isImmediate, fadeSpeed);
		}

		void SubmitInput()
		{
			if (string.IsNullOrWhiteSpace(inputField.text)) return;

			inputManager.OnSubmitInput?.Invoke(inputField.text.Trim());
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

		void ResetInput(string title)
		{
			titleText.text = title;
			inputField.text = string.Empty;

			if (submitButton.gameObject.activeSelf)
				submitButton.gameObject.SetActive(false);

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

		protected override IEnumerator FadeOut()
		{
			yield return base.FadeOut();
			OnClose?.Invoke();
		}
	}
}
