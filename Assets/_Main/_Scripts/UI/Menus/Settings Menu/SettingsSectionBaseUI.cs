using Game;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public abstract class SettingsSectionBaseUI : MonoBehaviour
	{
		[SerializeField] protected Button resetButton;
		[SerializeField] protected SettingsManager settingsManager;

		abstract public void Reset();
		abstract protected void SubscribeListeners();
		abstract protected void UnsubscribeListeners();

		void OnEnable()
		{
			SubscribeListeners();
		}

		void OnDisable()
		{
			UnsubscribeListeners();
		}
	}
}
