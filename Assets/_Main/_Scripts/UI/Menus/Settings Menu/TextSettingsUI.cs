using Dialogue;
using UnityEngine;
using UnityEngine.UI;
using VN;

namespace UI
{
	public class TextSettingsUI : SettingsSectionBaseUI
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] Button skipReadButton;
		[SerializeField] Button skipUnreadButton;
		[SerializeField] Button skipAfterChoicesButton;
		[SerializeField] CheckboxUI skipReadCheckbox;
		[SerializeField] CheckboxUI skipUnreadCheckbox;
		[SerializeField] CheckboxUI skipAfterChoicesCheckbox;
		[SerializeField] Slider textSpeedSlider;
		[SerializeField] Slider autoSpeedSlider;

		void Awake()
		{
			textSpeedSlider.minValue = vnOptions.Dialogue.MinTextSpeed;
			textSpeedSlider.maxValue = vnOptions.Dialogue.MaxTextSpeed;
			autoSpeedSlider.minValue = vnOptions.Dialogue.MinAutoSpeed;
			autoSpeedSlider.maxValue = vnOptions.Dialogue.MaxAutoSpeed;
		}

		void Start()
		{
			SetSkipModeCheckbox(settingsManager.SkipMode);
			textSpeedSlider.SetValueWithoutNotify(settingsManager.TextSpeed);
			autoSpeedSlider.SetValueWithoutNotify(settingsManager.AutoSpeed);
		}

		public override void Reset()
		{
			settingsManager.ResetSkipMode();
			settingsManager.ResetTextSpeed();
			settingsManager.ResetAutoSpeed();

			SetSkipModeCheckbox(settingsManager.SkipMode);
			textSpeedSlider.SetValueWithoutNotify(settingsManager.TextSpeed);
			autoSpeedSlider.SetValueWithoutNotify(settingsManager.AutoSpeed);
		}

		void SetReadSkipMode() => SetSkipMode(DialogueSkipMode.Read);
		void SetUnreadSkipMode() => SetSkipMode(DialogueSkipMode.Unread);
		void SetAfterChoicesSkipMode() => SetSkipMode(DialogueSkipMode.AfterChoices);
		void SetSkipMode(DialogueSkipMode skipMode)
		{
			if (skipMode == settingsManager.SkipMode) return;

			settingsManager.SetSkipMode(skipMode);
			SetSkipModeCheckbox(skipMode);
		}

		void SetSkipModeCheckbox(DialogueSkipMode skipMode)
		{
			switch (skipMode)
			{
				case DialogueSkipMode.Read:
					skipReadCheckbox.SetChecked(true);
					skipUnreadCheckbox.SetChecked(false);
					skipAfterChoicesCheckbox.SetChecked(false);
					break;
				case DialogueSkipMode.Unread:
					skipReadCheckbox.SetChecked(false);
					skipUnreadCheckbox.SetChecked(true);
					skipAfterChoicesCheckbox.SetChecked(false);
					break;
				case DialogueSkipMode.AfterChoices:
					skipReadCheckbox.SetChecked(false);
					skipUnreadCheckbox.SetChecked(false);
					skipAfterChoicesCheckbox.SetChecked(true);
					break;
			}
		}

		void SetTextSpeed(float speed)
		{
			settingsManager.SetTextSpeed(speed);
		}

		void SetAutoSpeed(float speed)
		{
			settingsManager.SetAutoSpeed(speed);
		}

		override protected void SubscribeListeners()
		{
			resetButton.onClick.AddListener(Reset);
			skipReadButton.onClick.AddListener(SetReadSkipMode);
			skipUnreadButton.onClick.AddListener(SetUnreadSkipMode);
			skipAfterChoicesButton.onClick.AddListener(SetAfterChoicesSkipMode);
			textSpeedSlider.onValueChanged.AddListener(SetTextSpeed);
			autoSpeedSlider.onValueChanged.AddListener(SetAutoSpeed);
		}

		override protected void UnsubscribeListeners()
		{
			resetButton.onClick.RemoveListener(Reset);
			skipReadButton.onClick.RemoveListener(SetReadSkipMode);
			skipUnreadButton.onClick.RemoveListener(SetUnreadSkipMode);
			skipAfterChoicesButton.onClick.RemoveListener(SetAfterChoicesSkipMode);
			textSpeedSlider.onValueChanged.RemoveListener(SetTextSpeed);
			autoSpeedSlider.onValueChanged.RemoveListener(SetAutoSpeed);
		}
	}
}
