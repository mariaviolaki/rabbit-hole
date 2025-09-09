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
			SetScreenModeCheckbox(gameStateManager.State.GameScreenMode);
			SetDropdownResolution(gameStateManager.State.ResolutionWidth, gameStateManager.State.ResolutionHeight);
			SetDropdownQuality(gameStateManager.State.GraphicsQuality);
		}

		public override void Reset()
		{
			gameStateManager.State.ResetScreenMode();
			gameStateManager.State.ResetResolution();
			gameStateManager.State.ResetGraphicsQuality();

			SetScreenModeCheckbox(gameStateManager.State.GameScreenMode);
			SetDropdownQuality(gameStateManager.State.GraphicsQuality);
			SetDropdownResolution(gameStateManager.State.ResolutionWidth, gameStateManager.State.ResolutionHeight);
		}

		void SetFullscreen() => SetScreenMode(ScreenMode.Fullscreen);
		void SetWindowed() => SetScreenMode(ScreenMode.Windowed);
		void SetScreenMode(ScreenMode screenMode)
		{
			if (screenMode == gameStateManager.State.GameScreenMode) return;

			gameStateManager.State.SetScreenMode(screenMode);
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
			if (gameStateManager.State.ResolutionWidth == resolution.width && gameStateManager.State.ResolutionHeight == resolution.height) return;

			gameStateManager.State.SetResolution(resolution.width, resolution.height);
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
			if (gameStateManager.State.GraphicsQuality == index) return;
			gameStateManager.State.SetGraphicsQuality(index);
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
