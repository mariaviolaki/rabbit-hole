using UnityEngine;

namespace UI
{
	public class GameplayControlsUI : FadeableUI
	{
		[SerializeField] InputPanelUI inputPanel;

		protected override void Awake()
		{
			base.Awake();

			inputPanel.OnClose += CloseInput;

			CloseInput();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			inputPanel.OnClose -= CloseInput;
		}

		public Coroutine ShowInput(string title, bool isImmediate = false, float fadeSpeed = 0)
		{
			inputPanel.gameObject.SetActive(true);

			return inputPanel.Show(title, isImmediate, fadeSpeed);
		}

		void CloseInput()
		{
			inputPanel.gameObject.SetActive(false);
		}
	}
}
