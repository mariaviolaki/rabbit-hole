using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class MenusUI : MonoBehaviour
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] SideMenuUI sideMenu;
		[SerializeField] SaveMenuUI saveMenu;
		[SerializeField] LogMenuUI logMenu;

		bool isTransitioning = false;

		void Awake()
		{
			inputManager.OnSideMenuOpen += OpenSideMenu;
			inputManager.OnMenuClose += CloseMenu;
			inputManager.OnOpenLog += OpenLogMenu;

			sideMenu.OnClose += CloseSideMenuRoot;
			logMenu.OnClose += CloseLogMenuRoot;
			saveMenu.OnClose += CloseSaveMenuRoot;
		}

		void Start()
		{
			CloseSideMenuRoot();
			CloseSaveMenuRoot();
			CloseLogMenuRoot();

			inputManager.CurrentMenu = MenuType.None;
			inputManager.IsChoicePanelOpen = false;
			inputManager.IsInputPanelOpen = false;
		}

		void OnDestroy()
		{
			inputManager.OnSideMenuOpen -= OpenSideMenu;
			inputManager.OnMenuClose -= CloseMenu;
			inputManager.OnOpenLog -= OpenLogMenu;

			sideMenu.OnClose -= CloseSideMenuRoot;
			logMenu.OnClose -= CloseLogMenuRoot;
			saveMenu.OnClose -= CloseSaveMenuRoot;
		}

		public void OpenSideMenu() => StartCoroutine(ShowSideMenu());
		IEnumerator ShowSideMenu(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (inputManager.CurrentMenu == MenuType.SideMenu || sideMenu.IsTransitioning || isTransitioning) yield break;
			isTransitioning = true;

			sideMenu.gameObject.SetActive(true);
			yield return sideMenu.Open(isImmediate, fadeSpeed);
			inputManager.CurrentMenu = MenuType.SideMenu;

			isTransitioning = false;
		}

		public void CloseSideMenu() => StartCoroutine(HideSideMenu());
		public IEnumerator HideSideMenu(bool isImmediate = false, float fadeSpeed = 0)
		{
			if (inputManager.CurrentMenu != MenuType.SideMenu || sideMenu.IsTransitioning || isTransitioning) yield break;
			isTransitioning = true;

			yield return sideMenu.Close(isImmediate, fadeSpeed);
			sideMenu.gameObject.SetActive(false);
			inputManager.CurrentMenu = MenuType.None;

			isTransitioning = false;
		}

		public void OpenMenu(MenuType menuType) => StartCoroutine(ShowMenu(menuType));

		IEnumerator ShowMenu(MenuType menuType, bool isImmediate = false, float fadeSpeed = 0)
		{
			bool isOnValidMenu = inputManager.CurrentMenu == MenuType.SideMenu || inputManager.CurrentMenu == MenuType.None;
			if (!isOnValidMenu || sideMenu.IsTransitioning || isTransitioning) yield break;
			isTransitioning = true;

			List<IEnumerator> transitions = new() { sideMenu.Close(isImmediate, fadeSpeed) };

			if (menuType == MenuType.Save)
			{
				saveMenu.gameObject.SetActive(true);
				transitions.Add(saveMenu.Open(SaveMenuMode.Save, isImmediate, fadeSpeed));
				yield return Utilities.RunConcurrentProcesses(this, transitions);
				inputManager.CurrentMenu = saveMenu.gameObject.activeInHierarchy ? MenuType.Save : MenuType.None;
			}
			else if (menuType == MenuType.Load)
			{
				saveMenu.gameObject.SetActive(true);
				transitions.Add(saveMenu.Open(SaveMenuMode.Load, isImmediate, fadeSpeed));
				yield return Utilities.RunConcurrentProcesses(this, transitions);
				inputManager.CurrentMenu = saveMenu.gameObject.activeInHierarchy ? MenuType.Load : MenuType.None;
			}
			else if (menuType == MenuType.Log)
			{
				logMenu.gameObject.SetActive(true);
				transitions.Add(logMenu.Open(isImmediate, fadeSpeed));
				yield return Utilities.RunConcurrentProcesses(this, transitions);
				inputManager.CurrentMenu = logMenu.gameObject.activeInHierarchy ? MenuType.Log : MenuType.None;
			}

			isTransitioning = false;
		}

		public void CloseMenu() => StartCoroutine(HideMenu());

		IEnumerator HideMenu(bool isImmediate = false, float transitionSpeed = 0f)
		{
			if (isTransitioning || sideMenu.IsTransitioning) yield break;
			isTransitioning = true;

			if (inputManager.CurrentMenu == MenuType.SideMenu)
			{
				if (sideMenu.IsTransitioning) yield break;
				yield return sideMenu.Close(isImmediate, transitionSpeed);
			}
			else if (inputManager.CurrentMenu == MenuType.Save || inputManager.CurrentMenu == MenuType.Load)
			{
				if (saveMenu.IsTransitioning) yield break;
				yield return saveMenu.Close(isImmediate, transitionSpeed);
			}
			else if (inputManager.CurrentMenu == MenuType.Log)
			{
				if (logMenu.IsTransitioning) yield break;
				yield return logMenu.Close(isImmediate, transitionSpeed);
			}
		}

		void OpenSaveMenu() => OpenMenu(MenuType.Save);
		void OpenLoadMenu() => OpenMenu(MenuType.Load);
		void OpenLogMenu() => OpenMenu(MenuType.Log);

		void CloseSideMenuRoot()
		{
			sideMenu.gameObject.SetActive(false);
			inputManager.CurrentMenu = MenuType.None;
			isTransitioning = false;
		}

		void CloseLogMenuRoot()
		{
			logMenu.gameObject.SetActive(false);
			inputManager.CurrentMenu = MenuType.None;
			isTransitioning = false;
		}

		void CloseSaveMenuRoot()
		{
			saveMenu.gameObject.SetActive(false);
			inputManager.CurrentMenu = MenuType.None;
			isTransitioning = false;
		}
	}
}
