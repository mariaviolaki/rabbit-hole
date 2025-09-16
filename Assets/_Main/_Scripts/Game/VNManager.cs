using Dialogue;
using Game;
using History;
using IO;
using UI;
using UnityEngine;

namespace VN
{
	public class VNManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] AssetManagerSO assetManager;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] HistoryManager historyManager;
		[SerializeField] DialogueManager dialogueManager;
		[SerializeField] SaveManager saveManager;
		[SerializeField] VisualNovelUI visualNovelUI;

		// Persistent global class
		GameManager gameManager;

		public GameManager Game => gameManager;
		public HistoryManager History => historyManager;
		public DialogueManager Dialogue => dialogueManager;
		public VNOptionsSO Options => vnOptions;
		public AssetManagerSO Assets => assetManager;
		public InputManagerSO Input => inputManager;
		public SaveManager Saving => saveManager;
		public VisualNovelUI UI => visualNovelUI;

		void OnEnable()
		{
			gameManager = FindObjectOfType<GameManager>();
		}

		void Start()
		{
			StartCoroutine(visualNovelUI.Show(false, vnOptions.General.SkipTransitionSpeed));
		}

		public void CompleteDialogue()
		{
			gameManager.ReturnToTitle();
		}
	}
}
