using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public abstract class SettingsSectionBaseUI : MonoBehaviour
	{
		protected Button resetButton;
		protected GameStateManager gameStateManager;

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

		public void InitCommonSettings(GameStateManager gameStateManager, Button resetButton)
		{
			this.gameStateManager = gameStateManager;
			this.resetButton = resetButton;
		}
	}
}
