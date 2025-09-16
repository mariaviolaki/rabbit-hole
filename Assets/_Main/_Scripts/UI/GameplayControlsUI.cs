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

		bool isTransitioning = false;

		protected override void Awake()
		{
			base.Awake();

			inputPanel.OnClose += CloseInputRoot;
			choicePanel.OnClose += CloseChoicesRoot;
		}

		void Start()
		{
			CloseInputRoot();
			CloseChoicesRoot();

			inputManager.IsChoicePanelOpen = false;
			inputManager.IsInputPanelOpen = false;
		}

		void OnDestroy()
		{
			inputPanel.OnClose -= CloseInputRoot;
			choicePanel.OnClose -= CloseChoicesRoot;
		}

		public IEnumerator ShowInput(string title, bool isImmediate = false, float fadeSpeed = 0)
		{
			inputPanel.gameObject.SetActive(true);

			yield return inputPanel.Open(title, isImmediate, fadeSpeed);
		}

		public IEnumerator ForceHideInput(bool isImmediate = false)
		{
			yield return inputPanel.Close(isImmediate);
		}

		public IEnumerator ShowChoices(List<DialogueChoice> choices, bool isImmediate = false, float fadeSpeed = 0)
		{
			choicePanel.gameObject.SetActive(true);

			yield return choicePanel.Open(choices, isImmediate, fadeSpeed);
		}

		public IEnumerator ForceHideChoices(bool isImmediate = false)
		{
			yield return choicePanel.Close(isImmediate);
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

			if (closeProcesses.Count > 0)
				yield return Utilities.RunConcurrentProcesses(this, closeProcesses);

			yield return base.SetHidden(isImmediate, speed);
			isTransitioning = false;
		}

		void CloseInputRoot() => inputPanel.gameObject.SetActive(false);
		void CloseChoicesRoot() => choicePanel.gameObject.SetActive(false);
	}
}
