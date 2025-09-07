using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SettingsMenuUI : FadeableUI
	{
		[SerializeField] Button displaySettingsButton;
		[SerializeField] Button textSettingsButton;
		[SerializeField] Button audioSettingsButton;
		[SerializeField] Color selectedImageColor;
		[SerializeField] Color unselectedImageColor;
		[SerializeField] Color selectedTextColor;
		[SerializeField] Color unselectedTextColor;
		[SerializeField] Button resetButton;
		[SerializeField] Button backButton;
		[SerializeField] DisplaySettingsUI displaySettings;
		[SerializeField] TextSettingsUI textSettings;
		[SerializeField] AudioSettingsUI audioSettings;
		[SerializeField] MenusUI menus;
		[SerializeField] GameStateManager gameStateManager;

		SettingsSection settingsSection = SettingsSection.None;
		bool isTransitioning = false;

		public event Action OnClose;

		public bool IsTransitioning => isTransitioning;

		override protected void Start()
		{
			base.Start();

			displaySettings.InitCommonSettings(gameStateManager, resetButton);
			textSettings.InitCommonSettings(gameStateManager, resetButton);
			audioSettings.InitCommonSettings(gameStateManager, resetButton);

			SelectDisplaySetting();
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

		public IEnumerator Open(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsVisible || isTransitioning) yield break;
			isTransitioning = true;

			base.fadeSpeed = fadeSpeed;
			isImmediateTransition = isImmediate;

			yield return SetVisible(isImmediate, fadeSpeed);

			isTransitioning = false;
		}

		public IEnumerator Close(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (IsHidden || isTransitioning) yield break;
			isTransitioning = true;

			gameStateManager.SavePlayerSettings();

			fadeSpeed = fadeSpeed <= 0 ? gameOptions.General.TransitionSpeed : fadeSpeed;
			yield return SetHidden(isImmediate, fadeSpeed);

			isTransitioning = false;
			OnClose?.Invoke();
		}

		void SelectDisplaySetting() => SelectSetting(SettingsSection.Display);
		void SelectTextSetting() => SelectSetting(SettingsSection.Text);
		void SelectAudioSetting() => SelectSetting(SettingsSection.Audio);
		void SelectSetting(SettingsSection settingsSection)
		{
			if (settingsSection == this.settingsSection) return;

			this.settingsSection = settingsSection;

			switch (settingsSection)
			{
				case SettingsSection.Display:
					UpdateSettingButton(displaySettingsButton, textSettingsButton, audioSettingsButton);
					UpdateSettingsSection(displaySettings, textSettings, audioSettings);
					break;
				case SettingsSection.Text:
					UpdateSettingButton(textSettingsButton, displaySettingsButton, audioSettingsButton);
					UpdateSettingsSection(textSettings, displaySettings, audioSettings);
					break;
				case SettingsSection.Audio:
					UpdateSettingButton(audioSettingsButton, displaySettingsButton, textSettingsButton);
					UpdateSettingsSection(audioSettings, displaySettings, textSettings);
					break;
			}
		}

		void UpdateSettingButton(Button selectedButton, params Button[] unselectedButtons)
		{
			selectedButton.image.color = selectedImageColor;
			selectedButton.GetComponentInChildren<TextMeshProUGUI>().color = selectedTextColor;

			foreach (Button button in unselectedButtons)
			{
				button.image.color = unselectedImageColor;
				button.GetComponentInChildren<TextMeshProUGUI>().color = unselectedTextColor;
			}
		}

		void UpdateSettingsSection(SettingsSectionBaseUI selectedSection, params SettingsSectionBaseUI[] unselectedSections)
		{
			foreach (SettingsSectionBaseUI section in unselectedSections)
				section.gameObject.SetActive(false);

			selectedSection.gameObject.SetActive(true);
		}

		void SubscribeListeners()
		{
			backButton.onClick.AddListener(menus.CloseMenu);
			displaySettingsButton.onClick.AddListener(SelectDisplaySetting);
			textSettingsButton.onClick.AddListener(SelectTextSetting);
			audioSettingsButton.onClick.AddListener(SelectAudioSetting);
		}

		void UnsubscribeListeners()
		{
			backButton.onClick.RemoveListener(menus.CloseMenu);
			displaySettingsButton.onClick.RemoveListener(SelectDisplaySetting);
			textSettingsButton.onClick.RemoveListener(SelectTextSetting);
			audioSettingsButton.onClick.RemoveListener(SelectAudioSetting);
		}
	}
}
