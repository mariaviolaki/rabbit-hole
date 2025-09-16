using Characters;
using IO;
using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VN;

namespace Game
{
	public class GameSceneManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] CharacterBankSO characterBank;
		[SerializeField] MenusUI menus;
		[SerializeField] GameManager gameManager;

		MainMenuUI mainMenu;
		VNManager vnManager;
		AsyncOperation loadProcess;

		GameScene currentScene;
		GameScene pendingScene;

		public event Action OnLoadSceneStart;
		public event Action OnLoadScene;

		public GameScene CurrentScene => currentScene;

		void Awake()
		{
			currentScene = GameScene.None;
			pendingScene = GameScene.None;
		}

		void OnEnable()
		{
			mainMenu = FindObjectOfType<MainMenuUI>();
			vnManager = FindObjectOfType<VNManager>();

			currentScene = mainMenu == null ? GameScene.VisualNovel : GameScene.MainMenu;
		}

		void Start()
		{
			LoadCharacters();
		}

		void OnDestroy()
		{
			UnloadCharacters();
		}

		public IEnumerator Load(GameScene gameScene)
		{
			bool isValidScene = (gameScene == GameScene.MainMenu && vnManager != null)
				|| (gameScene == GameScene.VisualNovel && mainMenu != null);

			if (loadProcess != null || pendingScene != GameScene.None || !isValidScene) yield break;

			OnLoadSceneStart?.Invoke();

			SceneManager.LoadScene(gameScene.ToString(), LoadSceneMode.Single);
			yield return null;

			currentScene = gameScene;
			SaveSceneReferences(currentScene);

			OnLoadScene?.Invoke();
		}

		public void StartLoading(GameScene gameScene)
		{
			bool isValidScene = (gameScene == GameScene.MainMenu && vnManager != null)
				|| (gameScene == GameScene.VisualNovel && mainMenu != null);

			if (loadProcess != null || pendingScene != GameScene.None || !isValidScene) return;

			pendingScene = gameScene;
			loadProcess = SceneManager.LoadSceneAsync(pendingScene.ToString(), LoadSceneMode.Additive);
			loadProcess.allowSceneActivation = false;
		}

		public void CompleteLoading()
		{
			if (loadProcess == null || pendingScene == GameScene.None) return;

			StartCoroutine(CompleteLoadingProcess());
		}

		IEnumerator CompleteLoadingProcess()
		{
			OnLoadSceneStart?.Invoke();

			loadProcess.allowSceneActivation = true;
			yield return null;

			while (loadProcess.progress < 0.9f) yield return null;

			SaveSceneReferences(pendingScene);

			AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(currentScene.ToString());
			while (!sceneUnload.isDone) yield return null;

			loadProcess = null;
			currentScene = pendingScene;
			pendingScene = GameScene.None;

			OnLoadScene?.Invoke();
		}

		void SaveSceneReferences(GameScene gameScene)
		{
			if (gameScene == GameScene.MainMenu)
				mainMenu = FindObjectOfType<MainMenuUI>();
			else if (gameScene == GameScene.VisualNovel)
				vnManager = FindObjectOfType<VNManager>();
		}

		void LoadCharacters()
		{
			foreach (var (shortName, data) in characterBank.Characters)
			{
				if (data.Type == CharacterType.Sprite)
					assetManager.LoadCharacterAtlas(shortName);
				else if (data.Type == CharacterType.Model3D)
					assetManager.LoadModel3DPrefab(shortName);
			}
		}

		void UnloadCharacters()
		{
			foreach (var (shortName, data) in characterBank.Characters)
			{
				if (data.Type == CharacterType.Sprite)
					assetManager.UnloadCharacterAtlas(shortName);
				else if (data.Type == CharacterType.Model3D)
					assetManager.UnloadModel3DPrefab(shortName);
			}
		}
	}
}
