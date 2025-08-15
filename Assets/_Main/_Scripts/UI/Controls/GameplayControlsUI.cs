using Dialogue;
using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class GameplayControlsUI : FadeableUI
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] InputPanelUI inputPanel;
		[SerializeField] ChoicePanelUI choicePanel;
		[SerializeField] LogPanelUI logPanel;

		bool isTransitioning = false;

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

		public IEnumerator ShowInput(string title, bool isImmediate = false, float fadeSpeed = 0)
		{
			inputPanel.gameObject.SetActive(true);

			yield return inputPanel.Open(title, isImmediate, fadeSpeed);
		}

		public IEnumerator ShowChoices(List<DialogueChoice> choices, bool isImmediate = false, float fadeSpeed = 0)
		{
			choicePanel.gameObject.SetActive(true);

			yield return choicePanel.Open(choices, isImmediate, fadeSpeed);
		}

		public void ShowLogDefault() => StartCoroutine(ShowLog());
		public IEnumerator ShowLog(bool isImmediate = false, float fadeSpeed = 0)
		{
			logPanel.gameObject.SetActive(true);

			yield return logPanel.Open(isImmediate, fadeSpeed);
		}

		public IEnumerator ForceHideInput(bool isImmediate = false)
		{
			yield return inputPanel.Close(isImmediate);
		}

		public IEnumerator ForceHideChoices(bool isImmediate = false)
		{
			yield return choicePanel.Close(isImmediate);
		}

		public IEnumerator HideLog(bool isImmediate = false, float fadeSpeed = 0)
		{
			yield return logPanel.Close(isImmediate, fadeSpeed);
		}

		public override IEnumerator SetHidden(bool isImmediate = false, float speed = 0)
		{
			if (isTransitioning) yield break;
			isTransitioning = true;

			List<IEnumerator> closeProcesses = new();

			if (inputPanel.gameObject.activeInHierarchy)
				closeProcesses.Add(inputPanel.Close(isImmediate, speed));
			if (choicePanel.gameObject.activeInHierarchy)
				closeProcesses.Add(choicePanel.Close(isImmediate, speed));
			if (logPanel.gameObject.activeInHierarchy)
				closeProcesses.Add(logPanel.Close(isImmediate, speed));

			if (closeProcesses.Count > 0)
				yield return Utilities.RunConcurrentProcesses(this, closeProcesses);

			yield return base.SetHidden(isImmediate, speed);
			isTransitioning = false;
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
