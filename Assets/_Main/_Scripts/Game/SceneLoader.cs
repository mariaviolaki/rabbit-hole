using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
	public class SceneLoader : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] MenusUI menus;
		[SerializeField] GameManager gameManager;

		MainMenuUI mainMenu;
		VisualNovelUI visualNovel;

		public event Action OnLoadScene;

		void Start()
		{
			mainMenu = FindObjectOfType<MainMenuUI>();
		}

		public void Load(GameScene gameScene)
		{
			if ((gameScene == GameScene.MainMenu && visualNovel == null) ||
				(gameScene == GameScene.VisualNovel && mainMenu == null))
				return;

			StartCoroutine(LoadSceneProcess(gameScene));
		}

		IEnumerator LoadSceneProcess(GameScene gameScene)
		{
			AsyncOperation loadProcess = SceneManager.LoadSceneAsync(gameScene.ToString());
			loadProcess.allowSceneActivation = false;

			List<IEnumerator> processes = new() { menus.Disable() };

			if (gameScene == GameScene.MainMenu)
				processes.Add(visualNovel.Hide(false, vnOptions.General.SceneFadeTransitionSpeed));
			else if (gameScene == GameScene.VisualNovel)
				processes.Add(mainMenu.Hide());

			yield return Utilities.RunConcurrentProcesses(this, processes);
			while (loadProcess.progress < 0.9f) yield return null;

			loadProcess.allowSceneActivation = true;
			yield return null;

			gameManager.Scene = gameScene;

			menus.Enable();
			if (gameScene == GameScene.MainMenu)
				mainMenu = FindObjectOfType<MainMenuUI>();
			else if (gameScene == GameScene.VisualNovel)
				visualNovel = FindObjectOfType<VisualNovelUI>();

			OnLoadScene?.Invoke();
		}
	}
}
