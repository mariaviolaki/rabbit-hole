using Audio;
using Dialogue;
using Game;
using History;
using IO;
using UI;
using UnityEngine;
using Variables;

namespace VN
{
	public class VNManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] HistoryManager historyManager;
		[SerializeField] DialogueManager dialogueManager;
		[SerializeField] VNStateManager vnStateManager;
		[SerializeField] VisualNovelUI visualNovelUI;

		// Persistent global classes
		GameManager gameManager;

		VariableManager variableManager;

		public GameManager Game => gameManager;
		public VariableManager Variables => variableManager;
		public VNStateManager StateManager => vnStateManager;
		public HistoryManager History => historyManager;
		public DialogueManager Dialogue => dialogueManager;
		public VNOptionsSO Options => vnOptions;
		public AssetManagerSO Assets => assetManager;
		public InputManagerSO Input => inputManager;
		public VisualNovelUI UI => visualNovelUI;

		void Awake()
		{
			gameManager = FindObjectOfType<GameManager>();			
			variableManager = new(gameManager.Progress);
		}

		void OnDestroy()
		{
			gameManager.Audio.StopAll(false, vnOptions.Audio.TransitionSpeed);
		}

		// TODO
		public void StartGame()
		{
			visualNovelUI.Show();
			dialogueManager.StartDialogue();
		}

		// TODO
		public void ContinueGame()
		{
			visualNovelUI.Show();
			dialogueManager.StartDialogue();
		}

		// TODO
		public void LoadGame()
		{
			dialogueManager.StartDialogue();
		}
	}
}
