using Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class GameplayControlsUI : BaseFadeableUI
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

		public Coroutine ShowChoices(List<DialogueChoice> choices, bool isImmediate = false, float fadeSpeed = 0)
		{
			choicePanel.gameObject.SetActive(true);

			return choicePanel.Show(choices, isImmediate, fadeSpeed);
		}

		public Coroutine ForceHideInput(bool isImmediate = false)
		{
			return inputPanel.ForceHide(isImmediate);
		}

		public Coroutine ForceHideChoices(bool isImmediate = false)
		{
			return choicePanel.ForceHide(isImmediate);
		}

		public override IEnumerator FadeOut(bool isImmediate = false, float speed = 0)
		{
			List<IEnumerator> closeProcesses = new()
			{
				inputPanel.CloseProcess(isImmediate, speed),
				choicePanel.CloseProcess(isImmediate, speed)
			};

			yield return Utilities.RunConcurrentProcesses(closeProcesses);
			yield return base.FadeOut(isImmediate, speed);
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
