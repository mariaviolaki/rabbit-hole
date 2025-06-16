using UnityEngine;

namespace UI
{
	public class GameplayControlsUI : FadeableUI
	{
		[SerializeField] InputPanelUI inputPanel;
		[SerializeField] ChoicePanelUI choicePanel;

		public InputPanelUI InputPanel => inputPanel;
		public ChoicePanelUI ChoicePanel => choicePanel;

		protected override void Awake()
		{
			base.Awake();

			inputPanel.OnClose += CloseInput;
			choicePanel.OnClose += CloseChoices;

			CloseInput();
			CloseChoices();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			inputPanel.OnClose -= CloseInput;
			choicePanel.OnClose -= CloseChoices;
		}

		public Coroutine ShowInput(string title, bool isImmediate = false, float fadeSpeed = 0)
		{
			inputPanel.gameObject.SetActive(true);

			return inputPanel.Show(title, isImmediate, fadeSpeed);
		}

		public Coroutine ShowChoices(string[] choices, bool isImmediate = false, float fadeSpeed = 0)
		{
			choicePanel.gameObject.SetActive(true);

			return choicePanel.Show(choices, isImmediate, fadeSpeed);
		}

		void CloseInput()
		{
			inputPanel.gameObject.SetActive(false);
		}

		void CloseChoices()
		{
			choicePanel.gameObject.SetActive(false);
		}
	}
}
