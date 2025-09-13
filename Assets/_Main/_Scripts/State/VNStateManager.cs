using Dialogue;
using Gameplay;
using History;
using IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Variables;

namespace VN
{
	public class VNStateManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] ScreenshotCamera screenshotCamera;
		[SerializeField] VNManager vnManager; // todo
		[SerializeField] HistoryManager historyManager;
		[SerializeField] DialogueManager dialogueManager;

		SaveFileManager saveFileManager;
		VariableManager variableManager;
		Coroutine saveFileCoroutine;
		string lastAutosaveNodeId;
		float autosaveTime = 0;

		void Start()
		{
			saveFileManager = vnManager.Game.SaveFiles;
			variableManager = vnManager.Variables;
		}

		void Update()
		{
			if (vnOptions.IO.HasAutosaveTimer && Time.time >= autosaveTime + vnOptions.IO.AutosaveTimerInterval)
				Autosave();
		}

		void OnDestroy()
		{
			Autosave();
		}

		public bool Autosave()
		{
			autosaveTime = Time.time;
			string currentDialogueNodeId = historyManager.HistoryState.Dialogue.DialogueNodeId;

			if (saveFileCoroutine != null || currentDialogueNodeId == null) return false; // unable to save
			if (currentDialogueNodeId == lastAutosaveNodeId) return true; // already saved

			if (!SaveGameplay(FilePaths.AutosaveSlot)) return false;

			lastAutosaveNodeId = currentDialogueNodeId;
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

		public bool SaveSlot(int slot)
		{
			if (saveFileCoroutine != null || slot < FilePaths.MinSaveSlot || slot > vnOptions.IO.SlotCount) return false;
			return SaveGameplay(slot);
		}

		public bool LoadSlot(int slot)
		{
			if (saveFileCoroutine != null || slot < FilePaths.MinSaveSlot || slot > vnOptions.IO.SlotCount) return false;

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
	}
}
