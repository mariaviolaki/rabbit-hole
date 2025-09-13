using Game;
using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class MenusUI : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] GameManager gameManager;

		SideMenuUI sideMenu;
		SettingsMenuUI settingsMenu;
		SaveMenuUI saveMenu;
		LogMenuUI logMenu;
		TraitsMenuUI traitsMenu;

		bool isEnabled = true;
		bool isTransitioning = false;

		MenuType DefaultMenu => gameManager.Scene == GameScene.VisualNovel ? MenuType.Dialogue : MenuType.Title;

		void Awake()
		{			
			inputManager.OnMenuClose += CloseMenu;
		}

		void Start()
		{
			inputManager.CurrentMenu = MenuType.Title;
			inputManager.IsChoicePanelOpen = false;
			inputManager.IsInputPanelOpen = false;
		}

		void OnDestroy()
		{			
			inputManager.OnMenuClose -= CloseMenu;
		}

		public void Enable() => isEnabled = true;
		public IEnumerator Disable()
		{
			isEnabled = false;
			isTransitioning = true;

			while (inputManager.CurrentMenu != DefaultMenu)
				yield return HideMenu(false, vnOptions.General.TransitionSpeed);

			isTransitioning = false;
		}

		public void OpenMenu(MenuType menuType)
		{
			if (!isEnabled) return;
			StartCoroutine(ShowMenu(menuType));
		}

		IEnumerator ShowMenu(MenuType menuType, bool isImmediate = false, float fadeSpeed = 0)
		{
			bool isInVNState = gameManager.Scene == GameScene.VisualNovel
				&& (inputManager.CurrentMenu == MenuType.Dialogue || inputManager.CurrentMenu == MenuType.SideMenu);
			bool isInMainState = gameManager.Scene == GameScene.MainMenu
				&& inputManager.CurrentMenu == MenuType.Title;

			bool isInValidState = menuType != inputManager.CurrentMenu && (isInVNState || isInMainState);
			if (!isInValidState || isTransitioning) yield break;

			MenuBaseUI menu = GetMenuFromType(menuType);
			if (!menu || !menu.IsTransitioning ) yield break;

			isTransitioning = true;

			List<IEnumerator> transitions = new();

			// Close the side menu if this menu is opened from inside the visual novel
			if (isInVNState && menuType != MenuType.SideMenu)
				transitions.Add(sideMenu.Close(isImmediate, fadeSpeed));

			menu.gameObject.SetActive(true);
			transitions.Add(menu.Open(menuType, isImmediate, fadeSpeed));
			yield return Utilities.RunConcurrentProcesses(this, transitions);
			inputManager.CurrentMenu = settingsMenu.gameObject.activeInHierarchy ? menuType : DefaultMenu;

			isTransitioning = false;
		}

		public void CloseMenu()
		{
			if (!isEnabled) return;
			StartCoroutine(HideMenu());
		}

		IEnumerator HideMenu(bool isImmediate = false, float transitionSpeed = 0f)
		{
			if (isTransitioning) yield break;

			MenuBaseUI menu = GetMenuFromType(inputManager.CurrentMenu);
			if (!menu || menu.IsTransitioning) yield break;

			isTransitioning = true;
			yield return menu.Close(isImmediate, transitionSpeed);
		}		

		public void SetMenu(MenuBaseUI menu)
		{
			switch (menu)
			{
				case SideMenuUI:
					sideMenu = (SideMenuUI)menu;
					inputManager.OnSideMenuOpen += OpenSideMenu;
					sideMenu.OnClose += CloseSideMenuRoot;
					CloseSideMenuRoot();
					break;
				case SettingsMenuUI:
					settingsMenu = (SettingsMenuUI)menu;
					settingsMenu.OnClose += CloseSettingsMenuRoot;
					CloseSettingsMenuRoot();
					break;
				case SaveMenuUI:
					saveMenu = (SaveMenuUI)menu;
					saveMenu.OnClose += CloseSaveMenuRoot;
					CloseSaveMenuRoot();
					break;
				case LogMenuUI:
					logMenu = (LogMenuUI)menu;
					inputManager.OnOpenLog += OpenLogMenu;
					logMenu.OnClose += CloseLogMenuRoot;
					CloseLogMenuRoot();
					break;
				case TraitsMenuUI:
					traitsMenu = (TraitsMenuUI)menu;
					traitsMenu.OnClose += CloseTraitsMenuRoot;
					CloseTraitsMenuRoot();
					break;
			}
		}

		public void RemoveMenu(MenuBaseUI menu)
		{
			switch (menu)
			{
				case SideMenuUI:
					inputManager.OnSideMenuOpen -= OpenSideMenu;
					sideMenu.OnClose -= CloseSideMenuRoot;
					sideMenu = null;
					break;
				case SettingsMenuUI:
					settingsMenu.OnClose -= CloseSettingsMenuRoot;
					settingsMenu = null;
					break;
				case SaveMenuUI:
					saveMenu.OnClose -= CloseSaveMenuRoot;
					saveMenu = null;
					break;
				case LogMenuUI:
					inputManager.OnOpenLog -= OpenLogMenu;
					logMenu.OnClose -= CloseLogMenuRoot;
					logMenu = null;
					break;
				case TraitsMenuUI:
					traitsMenu.OnClose -= CloseTraitsMenuRoot;
					traitsMenu = null;
					break;
			}
		}

		void OpenSideMenu() => OpenMenu(MenuType.SideMenu);
		void OpenSettingsMenu() => OpenMenu(MenuType.Settings);
		void OpenLoadMenu() => OpenMenu(MenuType.Load);
		void OpenSaveMenu() => OpenMenu(MenuType.Save);
		void OpenLogMenu() => OpenMenu(MenuType.Log);
		void OpenTraitsMenu() => OpenMenu(MenuType.Traits);

		void CloseSideMenuRoot()
		{
			sideMenu.gameObject.SetActive(false);
			inputManager.CurrentMenu = DefaultMenu;
			isTransitioning = false;
		}

		void CloseSettingsMenuRoot()
		{
			settingsMenu.gameObject.SetActive(false);
			inputManager.CurrentMenu = DefaultMenu;
			isTransitioning = false;
		}

		void CloseSaveMenuRoot()
		{
			saveMenu.gameObject.SetActive(false);
			inputManager.CurrentMenu = DefaultMenu;
			isTransitioning = false;
		}

		void CloseLogMenuRoot()
		{
			logMenu.gameObject.SetActive(false);
			inputManager.CurrentMenu = DefaultMenu;
			isTransitioning = false;
		}

		void CloseTraitsMenuRoot()
		{
			traitsMenu.gameObject.SetActive(false);
			inputManager.CurrentMenu = DefaultMenu;
			isTransitioning = false;
		}

		MenuBaseUI GetMenuFromType(MenuType menuType)
		{
			switch (menuType)
			{
				case MenuType.SideMenu: return sideMenu;
				case MenuType.Settings: return settingsMenu;
				case MenuType.Save: return saveMenu;
				case MenuType.Load: return saveMenu;
				case MenuType.Log: return logMenu;
				case MenuType.Traits: return traitsMenu;
				default: return null;
			}
		}
	}
}
