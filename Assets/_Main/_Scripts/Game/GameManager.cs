using Dialogue;
using History;
using IO;
using UnityEngine;
using Variables;

public class GameManager : MonoBehaviour
{
	[SerializeField] GameOptionsSO gameOptions;
	[SerializeField] FileManagerSO fileManager;
	[SerializeField] HistoryManager historyManager;
	[SerializeField] DialogueManager dialogueManager;
	[SerializeField] GameStateManager gameStateManager;

	SaveFileManager saveFileManager;
	VariableManager variableManager;

	public GameStateManager StateManager => gameStateManager;
	public SaveFileManager SaveManager => saveFileManager;
	public VariableManager Variables => variableManager;
	public HistoryManager History => historyManager;
	public GameOptionsSO Options => gameOptions;

	void Awake()
	{
		saveFileManager = new(gameOptions);
		variableManager = new();
	}

	void Update()
	{
		// TODO start dialogue using other triggers
		if (Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			Debug.Log("Starting Dialogue");
			dialogueManager.StartDialogue();
		}
		else if (Input.GetKeyDown(KeyCode.Q))
		{
			Debug.Log("Quitting Game");
			Application.Quit();
		}
	}
}
