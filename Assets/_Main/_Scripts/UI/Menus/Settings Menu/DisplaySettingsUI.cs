using IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class DisplaySettingsUI : SettingsSectionBaseUI
	{
		[SerializeField] Button windowButton;
		[SerializeField] Button fullscreenButton;
		[SerializeField] CheckboxUI windowCheckbox;
		[SerializeField] CheckboxUI fullscreenCheckbox;
		[SerializeField] TMP_Dropdown resolutionDropdown;
		[SerializeField] TMP_Dropdown qualityDropdown;

		void Awake()
		{
			InitResolutionsDropdown();
			InitQualityDropdown();
		}

		void Start()
		{
			SetScreenModeCheckbox(settingsManager.GameScreenMode);
			SetDropdownResolution(settingsManager.ResolutionWidth, settingsManager.ResolutionHeight);
			SetDropdownQuality(settingsManager.GraphicsQuality);
		}

		public override void Reset()
		{
			settingsManager.ResetScreenMode();
			settingsManager.ResetResolution();
			settingsManager.ResetGraphicsQuality();

			SetScreenModeCheckbox(settingsManager.GameScreenMode);
			SetDropdownQuality(settingsManager.GraphicsQuality);
			SetDropdownResolution(settingsManager.ResolutionWidth, settingsManager.ResolutionHeight);
		}

		void SetFullscreen() => SetScreenMode(ScreenMode.Fullscreen);
		void SetWindowed() => SetScreenMode(ScreenMode.Windowed);
		void SetScreenMode(ScreenMode screenMode)
		{
			if (screenMode == settingsManager.GameScreenMode) return;

			settingsManager.SetScreenMode(screenMode);
			SetScreenModeCheckbox(screenMode);
		}

		void SetScreenModeCheckbox(ScreenMode screenMode)
		{
			if (screenMode == ScreenMode.Windowed)
			{
				windowCheckbox.SetChecked(true);
				fullscreenCheckbox.SetChecked(false);
			}
			else
			{
				windowCheckbox.SetChecked(false);
				fullscreenCheckbox.SetChecked(true);
			}
		}

		void SetResolutionSetting(int index)
		{
			Resolution resolution = Screen.resolutions[index];
			if (settingsManager.ResolutionWidth == resolution.width && settingsManager.ResolutionHeight == resolution.height) return;

			settingsManager.SetResolution(resolution.width, resolution.height);
		}

		void SetDropdownResolution(int width, int height)
		{
			int matchIndex = System.Array.FindIndex(Screen.resolutions, r => r.width == width && r.height == height);
			if (matchIndex < 0) matchIndex = 0;

			resolutionDropdown.value = matchIndex;
			resolutionDropdown.RefreshShownValue();
		}

		void SetQualitySetting(int index)
		{
			if (settingsManager.GraphicsQuality == index) return;
			settingsManager.SetGraphicsQuality(index);
		}

		void SetDropdownQuality(int index)
		{
			qualityDropdown.value = index;
			qualityDropdown.RefreshShownValue();
		}

		void InitResolutionsDropdown()
		{
			Resolution[] resolutions = Screen.resolutions;
			List<string> options = new();

			for (int i = 0; i < resolutions.Length; i++)
			{
				options.Add($"{resolutions[i].width}x{resolutions[i].height}");
			}

			resolutionDropdown.ClearOptions();
			resolutionDropdown.AddOptions(options);
		}

		void InitQualityDropdown()
		{
			List<string> options = new(QualitySettings.names);

			qualityDropdown.ClearOptions();
			qualityDropdown.AddOptions(options);
		}

		override protected void SubscribeListeners()
		{
			resetButton.onClick.AddListener(Reset);
			windowButton.onClick.AddListener(SetWindowed);
			fullscreenButton.onClick.AddListener(SetFullscreen);
			resolutionDropdown.onValueChanged.AddListener(SetResolutionSetting);
			qualityDropdown.onValueChanged.AddListener(SetQualitySetting);
		}

		override protected void UnsubscribeListeners()
		{
			resetButton.onClick.RemoveListener(Reset);
			windowButton.onClick.RemoveListener(SetWindowed);
			fullscreenButton.onClick.RemoveListener(SetFullscreen);
			resolutionDropdown.onValueChanged.RemoveListener(SetResolutionSetting);
			qualityDropdown.onValueChanged.RemoveListener(SetQualitySetting);
		}
	}
}
