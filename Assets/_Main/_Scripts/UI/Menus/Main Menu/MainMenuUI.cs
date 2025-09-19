using Game;
using IO;
using System.Collections;
using UnityEngine;

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
		[SerializeField] SaveFileManagerSO saveFileManager;

		const string HidingAnimationName = "Hiding";
		const string HideAnimationTriggerName = "Hide";

		GameManager gameManager;
		MenusUI menus;
		bool hasAutosave;

		void OpenSettingsMenu() => menus.OpenMenu(MenuType.Settings);
		void OpenLoadMenu() => menus.OpenMenu(MenuType.Load);
		void OpenGalleryMenu() => menus.OpenMenu(MenuType.Gallery);

		void Awake()
		{
			hasAutosave = saveFileManager.HasAutosave();
		}

		void Start()
		{
			gameManager = FindObjectOfType<GameManager>();
			menus = gameManager.Menus;

			startButton.OnMainMenuAction += gameManager.StartGame;
			continueButton.OnMainMenuAction += gameManager.ContinueGame;
			quitButton.OnMainMenuAction += gameManager.QuitGame;

			loadButton.OnMainMenuAction += OpenLoadMenu;
			settingsButton.OnMainMenuAction += OpenSettingsMenu;
			galleryButton.OnMainMenuAction += OpenGalleryMenu;
		}

		void OnDestroy()
		{
			startButton.OnMainMenuAction -= gameManager.StartGame;
			continueButton.OnMainMenuAction -= gameManager.ContinueGame;
			quitButton.OnMainMenuAction -= gameManager.QuitGame;

			loadButton.OnMainMenuAction -= OpenLoadMenu;
			settingsButton.OnMainMenuAction -= OpenSettingsMenu;
			galleryButton.OnMainMenuAction -= OpenGalleryMenu;
		}

		public IEnumerator Hide()
		{
			animator.SetTrigger(HideAnimationTriggerName);

			// Wait for the hide animation to start
			while (!animator.GetCurrentAnimatorStateInfo(0).IsName(HidingAnimationName)) yield return null;
			// Wait for the hide animation to end
			while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
		}

		public bool IsActionAvailable(MainMenuAction action)
		{
			return action switch
			{
				MainMenuAction.Continue => hasAutosave,
				_ => true,
			};
		}
	}
}
