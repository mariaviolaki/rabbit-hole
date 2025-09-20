using Audio;
using IO;
using UI;
using UnityEngine;
using Variables;
using VN;

namespace Game
{
	public class GameManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] SaveFileManagerSO saveFileManager;
		[SerializeField] GameSceneManager sceneManager;
		[SerializeField] SettingsManager settingsManager;
		[SerializeField] GameProgressManager gameProgressManager;
		[SerializeField] AudioManager audioManager;
		[SerializeField] LoadManager loadManager;
		[SerializeField] VariableManager variableManager;
		[SerializeField] MenusUI menus;
		[SerializeField] ConfirmationMenuUI confirmationMenu;

		VNManager vnManager;
		MainMenuManager mainMenuManager;
		int pendingLoadSlot;

		public GameSceneManager Scenes => sceneManager;
		public SettingsManager Settings => settingsManager;
		public GameProgressManager Progress => gameProgressManager;
		public AudioManager Audio => audioManager;
		public SaveFileManagerSO SaveFiles => saveFileManager;
		public MenusUI Menus => menus;
		public VariableManager Variables => variableManager;
		public VNManager VN => vnManager;

		void Awake()
		{
			pendingLoadSlot = -1;
		}

		void Start()
		{
			if (sceneManager.CurrentScene == GameScene.VisualNovel)
				vnManager = FindAnyObjectByType<VNManager>();
			else if (sceneManager.CurrentScene == GameScene.MainMenu)
				mainMenuManager = FindAnyObjectByType<MainMenuManager>();

			sceneManager.OnLoadScene += ProcessSceneChange;

			sceneManager.StartLoading(GameScene.VisualNovel);
		}

		void OnDestroy()
		{
			sceneManager.OnLoadScene -= ProcessSceneChange;
		}

		public void StartGame()
		{
			pendingLoadSlot = -1;
			sceneManager.CompleteLoading();
		}

		public void ContinueGame()
		{
			pendingLoadSlot = FilePaths.AutosaveSlot;
			sceneManager.CompleteLoading();
		}

		public void LoadGame(int slot)
		{
			if (sceneManager.CurrentScene == GameScene.MainMenu)
			{
				pendingLoadSlot = slot;
				sceneManager.CompleteLoading();
			}
			else
			{
				loadManager.Load(slot);
			}
		}

		public void ReturnToTitle()
		{
			StartCoroutine(sceneManager.Load(GameScene.MainMenu));
		}

		public void QuitGame()
		{
			confirmationMenu.Open("Quit Game?", () => Application.Quit());
		}

		void ProcessSceneChange()
		{
			if (sceneManager.CurrentScene == GameScene.VisualNovel)
			{
				vnManager = FindAnyObjectByType<VNManager>();
				loadManager.Load(pendingLoadSlot);
				pendingLoadSlot = -1;
			}
			else if (sceneManager.CurrentScene == GameScene.MainMenu)
			{
				mainMenuManager = FindAnyObjectByType<MainMenuManager>();
				sceneManager.StartLoading(GameScene.VisualNovel);
			}
		}
	}
}
