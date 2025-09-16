using Dialogue;
using Game;
using Gameplay;
using History;
using IO;
using System;
using UnityEngine;
using Variables;

namespace VN
{
	public class SaveManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] SaveFileManagerSO saveFileManager;
		[SerializeField] ScreenshotCamera screenshotCamera;
		[SerializeField] VNManager vnManager;
		[SerializeField] HistoryManager historyManager;
		[SerializeField] DialogueManager dialogueManager;

		GameSceneManager sceneManager;
		VariableManager variableManager;
		string lastAutosaveNodeId;
		float autosaveTime = 0;

		void Start()
		{
			sceneManager = FindObjectOfType<GameSceneManager>();
			variableManager = vnManager.Game.Variables;

			sceneManager.OnLoadSceneStart += AutosaveBeforeSceneChange;
		}

		void Update()
		{
			if (vnOptions.IO.HasAutosaveTimer && Time.time >= autosaveTime + vnOptions.IO.AutosaveTimerInterval)
				Autosave();
		}

		void OnDestroy()
		{
			sceneManager.OnLoadSceneStart -= AutosaveBeforeSceneChange;

			// TODO Trigger this when taking showing the confirmation message instead of here
			//Autosave();
		}

		public bool Autosave()
		{
			if (historyManager.HistoryState == null) return false;

			autosaveTime = Time.time;
			string currentDialogueNodeId = historyManager.HistoryState.Dialogue.DialogueNodeId;

			if (currentDialogueNodeId == null) return false; // unable to save
			if (currentDialogueNodeId == lastAutosaveNodeId) return true; // already saved

			if (!SaveGameplay(FilePaths.AutosaveSlot)) return false;

			lastAutosaveNodeId = currentDialogueNodeId;
			return true;
		}

		public bool SaveSlot(int slot)
		{
			if (slot < FilePaths.MinSaveSlot || slot > vnOptions.IO.SlotCount) return false;
			return SaveGameplay(slot);
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

		void AutosaveBeforeSceneChange()
		{
			Autosave();
		}
	}
}
