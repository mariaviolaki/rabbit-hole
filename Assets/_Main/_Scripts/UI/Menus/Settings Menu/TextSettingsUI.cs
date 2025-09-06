using Dialogue;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class TextSettingsUI : SettingsSectionBaseUI
	{
		[SerializeField] GameOptionsSO gameOptions;
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
			textSpeedSlider.minValue = gameOptions.Dialogue.MinTextSpeed;
			textSpeedSlider.maxValue = gameOptions.Dialogue.MaxTextSpeed;
			autoSpeedSlider.minValue = gameOptions.Dialogue.MinAutoSpeed;
			autoSpeedSlider.maxValue = gameOptions.Dialogue.MaxAutoSpeed;
		}

		void Start()
		{
			SetSkipModeCheckbox(gameStateManager.State.SkipMode);
			textSpeedSlider.SetValueWithoutNotify(gameStateManager.State.TextSpeed);
			autoSpeedSlider.SetValueWithoutNotify(gameStateManager.State.AutoSpeed);
		}

		public override void Reset()
		{
			gameStateManager.State.ResetSkipMode();
			gameStateManager.State.ResetTextSpeed();
			gameStateManager.State.ResetAutoSpeed();

			SetSkipModeCheckbox(gameStateManager.State.SkipMode);
			textSpeedSlider.SetValueWithoutNotify(gameStateManager.State.TextSpeed);
			autoSpeedSlider.SetValueWithoutNotify(gameStateManager.State.AutoSpeed);
		}

		void SetReadSkipMode() => SetSkipMode(DialogueSkipMode.Read);
		void SetUnreadSkipMode() => SetSkipMode(DialogueSkipMode.Unread);
		void SetAfterChoicesSkipMode() => SetSkipMode(DialogueSkipMode.AfterChoices);
		void SetSkipMode(DialogueSkipMode skipMode)
		{
			if (skipMode == gameStateManager.State.SkipMode) return;

			gameStateManager.State.SetSkipMode(skipMode);
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
			gameStateManager.State.SetTextSpeed(speed);
		}

		void SetAutoSpeed(float speed)
		{
			gameStateManager.State.SetAutoSpeed(speed);
		}

		override protected void SubscribeListeners()
		{
			skipReadButton.onClick.AddListener(SetReadSkipMode);
			skipUnreadButton.onClick.AddListener(SetUnreadSkipMode);
			skipAfterChoicesButton.onClick.AddListener(SetAfterChoicesSkipMode);
			textSpeedSlider.onValueChanged.AddListener(SetTextSpeed);
			autoSpeedSlider.onValueChanged.AddListener(SetAutoSpeed);
		}

		override protected void UnsubscribeListeners()
		{
			skipReadButton.onClick.RemoveListener(SetReadSkipMode);
			skipUnreadButton.onClick.RemoveListener(SetUnreadSkipMode);
			skipAfterChoicesButton.onClick.RemoveListener(SetAfterChoicesSkipMode);
			textSpeedSlider.onValueChanged.RemoveListener(SetTextSpeed);
			autoSpeedSlider.onValueChanged.RemoveListener(SetAutoSpeed);
		}
	}
}
