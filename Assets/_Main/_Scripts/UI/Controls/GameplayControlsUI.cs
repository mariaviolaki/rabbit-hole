using Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class GameplayControlsUI : BaseFadeableUI
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] InputPanelUI inputPanel;
		[SerializeField] ChoicePanelUI choicePanel;
		[SerializeField] LogPanelUI logPanel;

		protected override void Awake()
		{
			base.Awake();

			inputManager.OnOpenLog += ShowLogDefault;

			inputPanel.OnClose += CloseInput;
			choicePanel.OnClose += CloseChoices;
			logPanel.OnClose += CloseLog;
		}

		protected override void Start()
		{
			base.Start();

			CloseInput();
			CloseChoices();
			CloseLog();

			inputManager.IsChoicePanelOpen = false;
			inputManager.IsInputPanelOpen = false;
			inputManager.IsLogPanelOpen = false;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			inputPanel.OnClose -= CloseInput;
			choicePanel.OnClose -= CloseChoices;
			logPanel.OnClose -= CloseLog;
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

		public void ShowLogDefault() => ShowLog();
		public Coroutine ShowLog(bool isImmediate = false, float fadeSpeed = 0)
		{
			logPanel.gameObject.SetActive(true);

			return logPanel.Show(isImmediate, fadeSpeed);
		}

		public Coroutine ForceHideInput(bool isImmediate = false)
		{
			return inputPanel.ForceHide(isImmediate);
		}

		public Coroutine ForceHideChoices(bool isImmediate = false)
		{
			return choicePanel.ForceHide(isImmediate);
		}

		public Coroutine HideLog(bool isImmediate = false, float fadeSpeed = 0)
		{
			return logPanel.Hide(isImmediate, fadeSpeed);
		}

		public override IEnumerator FadeOut(bool isImmediate = false, float speed = 0)
		{
			List<IEnumerator> closeProcesses = new()
			{
				inputPanel.CloseProcess(isImmediate, speed),
				choicePanel.CloseProcess(isImmediate, speed),
				logPanel.CloseProcess(isImmediate, speed)
			};

			yield return Utilities.RunConcurrentProcesses(this, closeProcesses);
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

		void CloseLog()
		{
			logPanel.gameObject.SetActive(false);
		}
	}
}
