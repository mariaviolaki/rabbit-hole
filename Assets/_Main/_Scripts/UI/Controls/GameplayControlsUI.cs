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
		[SerializeField] SideMenuUI sideMenu;
		[SerializeField] InputPanelUI inputPanel;
		[SerializeField] ChoicePanelUI choicePanel;
		[SerializeField] LogPanelUI logPanel;

		bool isTransitioning = false;

		protected override void Awake()
		{
			base.Awake();

			inputManager.OnSideMenuOpen += ShowSideMenuDefault;
			inputManager.OnMenuClose += CloseMenu;

			inputManager.OnOpenLog += ShowLogDefault;

			sideMenu.OnClose += CloseSideMenu;
			inputPanel.OnClose += CloseInput;
			choicePanel.OnClose += CloseChoices;
			logPanel.OnClose += CloseLog;
		}

		protected override void Start()
		{
			base.Start();

			CloseSideMenu();
			CloseInput();
			CloseChoices();
			CloseLog();

			inputManager.CurrentMenu = MenuType.None;
			inputManager.IsChoicePanelOpen = false;
			inputManager.IsInputPanelOpen = false;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			inputManager.OnSideMenuOpen -= ShowSideMenuDefault;
			inputManager.OnMenuClose -= CloseMenu;

			inputManager.OnOpenLog -= ShowLogDefault;

			sideMenu.OnClose -= CloseSideMenu;
			inputPanel.OnClose -= CloseInput;
			choicePanel.OnClose -= CloseChoices;
			logPanel.OnClose -= CloseLog;
		}

		public void CloseMenu()
		{
			if (inputManager.CurrentMenu == MenuType.SideMenu)
				HideSideMenuDefault();
			else if (inputManager.CurrentMenu == MenuType.Log)
				HideLogDefault();
		}

		public void ShowSideMenuDefault() => StartCoroutine(ShowSideMenu());
		public IEnumerator ShowSideMenu(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (inputManager.CurrentMenu == MenuType.SideMenu || sideMenu.IsTransitioning) yield break;

			sideMenu.gameObject.SetActive(true);
			yield return sideMenu.Open(isImmediate, fadeSpeed);
		}

		public void HideSideMenuDefault() => StartCoroutine(HideSideMenu());
		public IEnumerator HideSideMenu(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (inputManager.CurrentMenu != MenuType.SideMenu || sideMenu.IsTransitioning) yield break;

			yield return sideMenu.Close(isImmediate, fadeSpeed);
			sideMenu.gameObject.SetActive(false);
		}

		public void ShowLogDefault() => StartCoroutine(ShowLog());
		public IEnumerator ShowLog(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (inputManager.CurrentMenu == MenuType.Log || logPanel.IsTransitioning) yield break;

			logPanel.gameObject.SetActive(true);
			yield return logPanel.Open(isImmediate, fadeSpeed);
		}

		public void HideLogDefault() => StartCoroutine(HideLog());
		public IEnumerator HideLog(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (inputManager.CurrentMenu != MenuType.Log || logPanel.IsTransitioning) yield break;

			yield return logPanel.Close(isImmediate, fadeSpeed);
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
			if (logPanel.gameObject.activeInHierarchy)
				closeProcesses.Add(logPanel.Close(isImmediate, speed));

			if (closeProcesses.Count > 0)
				yield return Utilities.RunConcurrentProcesses(this, closeProcesses);

			yield return base.SetHidden(isImmediate, speed);
			isTransitioning = false;
		}

		void CloseSideMenu()
		{
			sideMenu.gameObject.SetActive(false);
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
