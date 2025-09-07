using Audio;
using Dialogue;
using Gameplay;
using History;
using IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Variables;

public class GameStateManager : MonoBehaviour
{
	[SerializeField] GameOptionsSO gameOptions;
	[SerializeField] FileManagerSO fileManager;
	[SerializeField] GameManager gameManager;
	[SerializeField] HistoryManager historyManager;
	[SerializeField] DialogueManager dialogueManager;
	[SerializeField] AudioManager audioManager;
	[SerializeField] ScreenshotCamera screenshotCamera;

	SaveFileManager saveFileManager;
	VariableManager variableManager;
	GameState gameState;
	Coroutine saveFileCoroutine;
	float autosaveTime = 0;

	public GameState State => gameState;

	void Awake()
	{
		gameState = new();
	}

	void Start()
	{
		saveFileManager = gameManager.SaveManager;
		variableManager = gameManager.Variables;

		gameState.Initialize(LoadPlayerSettings(), LoadPlayerProgress(), audioManager, gameOptions);
		variableManager.Initialize(this);
	}

	void Update()
	{
		if (gameOptions.IO.HasAutosaveTimer && Time.time >= autosaveTime + gameOptions.IO.AutosaveTimerInterval)
			Autosave();

		// TODO delete after creating UI
		if (Input.GetKeyDown(KeyCode.P))
		{
			Debug.Log("Saving Progress");
			SavePlayerProgress();
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

		if (!SaveGameplay(FilePaths.AutosaveSlot)) return false;

		gameState.LastAutosaveNodeId = currentDialogueNodeId;
		return true;
	}

	public bool LoadAutosave()
	{
		if (saveFileCoroutine != null || !saveFileManager.HasAutosave()) return false;

		SaveSlot saveSlot = saveFileManager.LoadSlot(FilePaths.AutosaveSlot);
		if (saveSlot == null || saveSlot.historyStates == null)
		{
			Debug.LogWarning($"Unable to repopulate history states because the autosave slot was invalid.");
			return false;
		}

		saveFileCoroutine = StartCoroutine(LoadSaveFileHistory(saveSlot.historyStates));
		return true;
	}

	public bool SavePlayerSettings()
	{
		if (!gameState.HasPendingSettings) return true;

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
		return SaveGameplay(slot);
	}

	public bool LoadSlot(int slot)
	{
		if (saveFileCoroutine != null || slot < FilePaths.MinSaveSlot || slot > gameOptions.IO.SlotCount) return false;

		SaveSlot saveSlot = saveFileManager.LoadSlot(slot);
		if (saveSlot == null || saveSlot.historyStates == null)
		{
			Debug.LogWarning($"Unable to repopulate history states because the loaded game save was invalid.");
			return false;
		}

		saveFileCoroutine = StartCoroutine(LoadSaveFileHistory(saveSlot.historyStates));
		return true;
	}

	bool SaveGameplay(int slot)
	{
		object routeObject = variableManager.Get(DefaultVariables.RouteVariable);
		string routeString = routeObject == null ? CharacterRoute.Common.ToString() : routeObject.ToString();
		CharacterRoute route = (CharacterRoute)Enum.Parse(typeof(CharacterRoute), routeString, ignoreCase: true);

		string sceneTitle = dialogueManager.FlowController.CurrentSceneTitle;

		Texture2D screenshot = screenshotCamera.CaptureForSlot();

		return saveFileManager.SaveSlot(slot, route, sceneTitle, historyManager.HistoryStates, screenshot);
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
