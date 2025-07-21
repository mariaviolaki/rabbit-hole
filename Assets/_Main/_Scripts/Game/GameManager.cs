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
	[SerializeField] string mainDialogueFile;

	DialogueLineBank dialogueLineBank;
	SaveFileManager saveFileManager;
	GameState gameState;
	Coroutine saveFileCoroutine;
	float autosaveTime = 0;

	public GameState State => gameState;
	public DialogueLineBank DialogueBank => dialogueLineBank;

	void Start()
	{
		saveFileManager = new();
		dialogueLineBank = new(this, dialogueManager, fileManager);
		gameState = new(LoadPlayerSettings(), LoadPlayerProgress(), dialogueLineBank, audioManager, gameOptions);
	}

	void Update()
	{
		if (gameOptions.IO.HasAutosaveTimer && Time.time >= autosaveTime + gameOptions.IO.AutosaveTimerInterval)
			Autosave();

		// TODO start dialogue using other triggers
		if (Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			dialogueManager.LoadDialogue(mainDialogueFile);
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
		int currentDialogueLineId = historyManager.CurrentDialogueLineId;

		if (saveFileCoroutine != null || currentDialogueLineId == 0) return false; // unable to save
		if (currentDialogueLineId == gameState.LastAutosaveLine) return true; // already saved

		if (!saveFileManager.SaveSlot(saveFileManager.AutosaveSlot, historyManager.GetSaveFileHistoryStates())) return false;

		Debug.Log("Autosave Success");

		gameState.LastAutosaveLine = currentDialogueLineId;
		return true;
	}

	public bool LoadAutosave()
	{
		if (saveFileCoroutine != null || !saveFileManager.HasAutosave()) return false;

		List<HistoryState> historyStates = saveFileManager.LoadSlot(saveFileManager.AutosaveSlot);
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
		if (saveFileCoroutine != null || slot < saveFileManager.MinSaveSlot || slot > gameOptions.IO.SlotCount) return false;

		return saveFileManager.SaveSlot(slot, historyManager.GetSaveFileHistoryStates());
	}

	public bool LoadSlot(int slot)
	{
		if (saveFileCoroutine != null || slot < saveFileManager.MinSaveSlot || slot > gameOptions.IO.SlotCount) return false;

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
