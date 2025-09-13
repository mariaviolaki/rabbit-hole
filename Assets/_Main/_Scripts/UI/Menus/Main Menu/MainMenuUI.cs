using Game;
using IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Variables;
using VN;

namespace UI
{
	public class MainMenuUI : MonoBehaviour
	{
		[SerializeField] Animator animator;
		[SerializeField] MainMenuButtonUI startButton;
		[SerializeField] MainMenuButtonUI continueButton;
		[SerializeField] MainMenuButtonUI loadButton;
		[SerializeField] MainMenuButtonUI galleryButton;
		[SerializeField] MainMenuButtonUI settingsButton;
		[SerializeField] MainMenuButtonUI quitButton;

		const string ShowingAnimationName = "Showing";
		const string VisibleAnimationName = "Visible";
		const string HidingAnimationName = "Hiding";
		const string HiddenAnimationName = "Hidden";
		const string HideAnimationTriggerName = "Hide";

		GameManager gameManager;
		MenusUI menus;

		void OpenSettingsMenu() => menus.OpenMenu(MenuType.Settings);
		void OpenLoadMenu() => menus.OpenMenu(MenuType.Load);

		void Start()
		{
			gameManager = FindObjectOfType<GameManager>();
			menus = FindObjectOfType<MenusUI>();

			startButton.OnMainMenuAction += gameManager.StartGame;
			continueButton.OnMainMenuAction += gameManager.ContinueGame;
			quitButton.OnMainMenuAction += gameManager.QuitGame;

			loadButton.OnMainMenuAction += OpenLoadMenu;
			settingsButton.OnMainMenuAction += OpenSettingsMenu;
			// TODO GALLERY MENU
		}

		void OnDestroy()
		{
			startButton.OnMainMenuAction -= gameManager.StartGame;
			continueButton.OnMainMenuAction -= gameManager.ContinueGame;
			quitButton.OnMainMenuAction -= gameManager.QuitGame;

			loadButton.OnMainMenuAction -= OpenLoadMenu;
			settingsButton.OnMainMenuAction -= OpenSettingsMenu;
			// TODO GALLERY MENU
		}

		public IEnumerator Hide()
		{
			animator.SetTrigger(HideAnimationTriggerName);

			// Wait for the hide animation to start
			while (!animator.GetCurrentAnimatorStateInfo(0).IsName(HidingAnimationName)) yield return null;
			// Wait for the hide animation to end
			while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
		}

		public bool IsActionAvailable(MainMenuAction action) // todo
		{
			switch (action)
			{
				case MainMenuAction.Continue: return false;
				default: return true;
			}
		}
	}
}
