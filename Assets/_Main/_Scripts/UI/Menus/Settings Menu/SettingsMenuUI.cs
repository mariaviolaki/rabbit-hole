using Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class SettingsMenuUI : MenuBaseUI
	{
		[SerializeField] Button displaySettingsButton;
		[SerializeField] Button textSettingsButton;
		[SerializeField] Button audioSettingsButton;
		[SerializeField] Color selectedImageColor;
		[SerializeField] Color unselectedImageColor;
		[SerializeField] Color selectedTextColor;
		[SerializeField] Color unselectedTextColor;
		[SerializeField] DisplaySettingsUI displaySettings;
		[SerializeField] TextSettingsUI textSettings;
		[SerializeField] AudioSettingsUI audioSettings;
		[SerializeField] SettingsManager settingsManager;

		SettingsSection settingsSection = SettingsSection.None;

		override protected void Start()
		{
			base.Start();
			SelectDisplaySetting();
		}

		protected override void CompleteClose()
		{
			base.CompleteClose();
			settingsManager.SavePlayerSettings();
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

		override protected void SubscribeListeners()
		{
			base.SubscribeListeners();
			displaySettingsButton.onClick.AddListener(SelectDisplaySetting);
			textSettingsButton.onClick.AddListener(SelectTextSetting);
			audioSettingsButton.onClick.AddListener(SelectAudioSetting);
		}

		override protected void UnsubscribeListeners()
		{
			base.UnsubscribeListeners();
			displaySettingsButton.onClick.RemoveListener(SelectDisplaySetting);
			textSettingsButton.onClick.RemoveListener(SelectTextSetting);
			audioSettingsButton.onClick.RemoveListener(SelectAudioSetting);
		}
	}
}
