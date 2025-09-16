using Game;
using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VN;

namespace UI
{
	public class MenusUI : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] SideMenuUI sideMenu;
		[SerializeField] SettingsMenuUI settingsMenu;
		[SerializeField] SaveMenuUI saveMenu;
		[SerializeField] LogMenuUI logMenu;
		[SerializeField] TraitsMenuUI traitsMenu;
		[SerializeField] GameManager gameManager;

		bool isEnabled = true;
		bool isTransitioning = false;

		MenuType DefaultMenu => gameManager.Scenes.CurrentScene == GameScene.VisualNovel ? MenuType.Dialogue : MenuType.Title;

		void Awake()
		{
			gameManager.Scenes.OnLoadScene += UpdateMenuOnSceneChange;

			inputManager.OnMenuClose += CloseMenu;
			inputManager.OnSideMenuOpen += OpenSideMenu;
			inputManager.OnOpenLog += OpenLogMenu;

			sideMenu.OnClose += CloseSideMenuRoot;
			settingsMenu.OnClose += CloseSettingsMenuRoot;
			saveMenu.OnClose += CloseSaveMenuRoot;
			logMenu.OnClose += CloseLogMenuRoot;
			traitsMenu.OnClose += CloseTraitsMenuRoot;

			inputManager.IsChoicePanelOpen = false;
			inputManager.IsInputPanelOpen = false;
		}

		void Start()
		{
			UpdateMenuOnSceneChange();
		}

		void OnDestroy()
		{
			gameManager.Scenes.OnLoadScene -= UpdateMenuOnSceneChange;

			inputManager.OnMenuClose -= CloseMenu;
			inputManager.OnSideMenuOpen -= OpenSideMenu;
			inputManager.OnOpenLog -= OpenLogMenu;

			sideMenu.OnClose -= CloseSideMenuRoot;
			settingsMenu.OnClose -= CloseSettingsMenuRoot;
			saveMenu.OnClose -= CloseSaveMenuRoot;
			logMenu.OnClose -= CloseLogMenuRoot;
			traitsMenu.OnClose -= CloseTraitsMenuRoot;
		}

		public void OpenMenu(MenuType menuType)
		{
			if (!isEnabled) return;

			if (menuType == MenuType.Title)
				ReturnToTitle();
			else
				StartCoroutine(ShowMenu(menuType));
		}

		IEnumerator ShowMenu(MenuType menuType, bool isImmediate = false, float fadeSpeed = 0)
		{
			bool isInVNState = gameManager.Scenes.CurrentScene == GameScene.VisualNovel
				&& (inputManager.CurrentMenu == MenuType.Dialogue || inputManager.CurrentMenu == MenuType.SideMenu);
			bool isInMainState = gameManager.Scenes.CurrentScene == GameScene.MainMenu
				&& inputManager.CurrentMenu == MenuType.Title;

			bool isInValidState = menuType != inputManager.CurrentMenu && (isInVNState || isInMainState);
			if (!isInValidState || isTransitioning) yield break;

			MenuBaseUI menu = GetMenuFromType(menuType);
			if (!menu || menu.IsTransitioning) yield break;

			isTransitioning = true;
			List<IEnumerator> transitions = new();

			// Close the side menu if this menu is opened from inside the visual novel
			if (isInVNState && menuType != MenuType.SideMenu)
				transitions.Add(sideMenu.Close(isImmediate, fadeSpeed));

			menu.gameObject.SetActive(true);
			transitions.Add(menu.Open(menuType, isImmediate, fadeSpeed));
			yield return Utilities.RunConcurrentProcesses(this, transitions);

			inputManager.CurrentMenu = menu.gameObject.activeInHierarchy ? menuType : DefaultMenu;
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

		void ReturnToTitle()
		{
			isEnabled = false;

			StartCoroutine(HideMenu(false, vnOptions.General.SceneFadeTransitionSpeed));
			gameManager.ReturnToTitle();

			isEnabled = true;
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

		void UpdateMenuOnSceneChange()
		{
			inputManager.CurrentMenu = DefaultMenu;
		}
	}
}
