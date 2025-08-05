using Audio;
using Dialogue;
using History;
using IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] GameOptionsSO gameOptions;
	[SerializeField] FileManagerSO fileManager;
	[SerializeField] HistoryManager historyManager;
	[SerializeField] DialogueManager dialogueManager;
	[SerializeField] AudioManager audioManager;

	SaveFileManager saveFileManager;
	GameState gameState;
	Coroutine saveFileCoroutine;
	float autosaveTime = 0;

	public GameState State => gameState;
	public SaveFileManager SaveManager => saveFileManager;
	public HistoryManager History => historyManager;
	public GameOptionsSO Options => gameOptions;

	void Start()
	{
		saveFileManager = new();
		gameState = new(LoadPlayerSettings(), LoadPlayerProgress(), audioManager, gameOptions);
	}

	void Update()
	{
		if (gameOptions.IO.HasAutosaveTimer && Time.time >= autosaveTime + gameOptions.IO.AutosaveTimerInterval)
			Autosave();

		// TODO start dialogue using other triggers
		if (Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			Debug.Log("Starting Dialogue");
			dialogueManager.StartDialogue();
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			Debug.Log("Saving Slot");
			SaveSlot(1);
		}
		else if (Input.GetKeyDown(KeyCode.L))
		{
			Debug.Log("Loading Slot");
			LoadSlot(1);
		}
		else if (Input.GetKeyDown(KeyCode.Q))
		{
			Debug.Log("Quitting Game");
			Application.Quit();
		}
		else if (Input.GetKeyDown(KeyCode.P))
		{
			Debug.Log("Saving Progress");
			SavePlayerProgress();
		}
		else if (Input.GetKeyDown(KeyCode.O))
		{
			Debug.Log("Closing Options Menu");
			SavePlayerSettings();
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			Debug.Log("Making Options Changes");
			gameState.SetTextSpeed(10);
			gameState.SetVolume(0);
		}
		else if (Input.GetKeyDown(KeyCode.C))
		{
			Debug.Log("Continuing");
			LoadAutosave();
		}
	}

	void OnApplicationQuit()
	{
		Autosave();
		SavePlayerSettings();
		SavePlayerProgress();
	}

	public bool Autosave()
	{
		autosaveTime = Time.time;
		string currentDialogueNodeId = historyManager.HistoryState.Dialogue.DialogueNodeId;

		if (saveFileCoroutine != null || currentDialogueNodeId == null) return false; // unable to save
		if (currentDialogueNodeId == gameState.LastAutosaveNodeId) return true; // already saved

		if (!saveFileManager.SaveSlot(FilePaths.AutosaveSlot, historyManager.HistoryStates)) return false;

		Debug.Log("Autosave Success");

		gameState.LastAutosaveNodeId = currentDialogueNodeId;
		return true;
	}

	public bool LoadAutosave()
	{
		if (saveFileCoroutine != null || !saveFileManager.HasAutosave()) return false;

		List<HistoryState> historyStates = saveFileManager.LoadSlot(FilePaths.AutosaveSlot);
		if (historyStates == null)
		{
			Debug.LogWarning($"Unable to repopulate history states because the autosave slot was invalid.");
			return false;
		}

		saveFileCoroutine = StartCoroutine(LoadSaveFileHistory(historyStates));
		return true;
	}

	public bool SavePlayerSettings()
	{
		if (!gameState.HasPendingSettings) return true;

		Debug.Log("Saving Settings To File");
		if (saveFileManager.SavePlayerSettings(gameState.Settings))
		{
			gameState.HasPendingSettings = false;
			return true;
		}

		return false;
	}

	public bool SavePlayerProgress()
	{
		if (!gameState.HasPendingProgress) return true;

		if (saveFileManager.SavePlayerProgress(gameState.Progress))
		{
			gameState.HasPendingProgress = false;
			return true;
		}

		return false;
	}

	public bool SaveSlot(int slot)
	{
		if (saveFileCoroutine != null || slot < FilePaths.MinSaveSlot || slot > gameOptions.IO.SlotCount) return false;

		return saveFileManager.SaveSlot(slot, historyManager.HistoryStates);
	}

	public bool LoadSlot(int slot)
	{
		if (saveFileCoroutine != null || slot < FilePaths.MinSaveSlot || slot > gameOptions.IO.SlotCount) return false;

		List<HistoryState> historyStates = saveFileManager.LoadSlot(slot);
		if (historyStates == null)
		{
			Debug.LogWarning($"Unable to repopulate history states because the loaded game save was invalid.");
			return false;
		}

		saveFileCoroutine = StartCoroutine(LoadSaveFileHistory(historyStates));
		return true;
	}

	IEnumerator LoadSaveFileHistory(List<HistoryState> historyStates)
	{
		yield return historyManager.ApplySaveFileHistory(historyStates);
		saveFileCoroutine = null;
	}

	PlayerSettings LoadPlayerSettings()
	{
		PlayerSettings playerSettings;
		if (saveFileManager.HasSettingsSave())
		{
			playerSettings = saveFileManager.LoadPlayerSettings();
			if (playerSettings != null) return playerSettings;
		}

		playerSettings = new(gameOptions);
		saveFileManager.SavePlayerSettings(playerSettings);

		return playerSettings;
	}

	PlayerProgress LoadPlayerProgress()
	{
		PlayerProgress playerProgress;
		if (saveFileManager.HasProgressSave())
		{
			playerProgress = saveFileManager.LoadPlayerProgress();
			if (playerProgress != null) return playerProgress;
		}

		playerProgress = new();
		saveFileManager.SavePlayerProgress(playerProgress);

		return playerProgress;
	}
}
