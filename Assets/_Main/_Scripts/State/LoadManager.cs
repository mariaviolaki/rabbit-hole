using IO;
using System.Collections;
using UnityEngine;
using VN;

namespace Game
{
	public class LoadManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] SaveFileManagerSO saveFileManager;

		VNManager vnManager;
		Coroutine saveFileCoroutine;
		SaveSlot currentSlot;

		public bool Load(int slot = -1)
		{
			if (saveFileCoroutine != null) return false;

			bool isValidSlot = slot >= FilePaths.AutosaveSlot && slot <= vnOptions.IO.SlotCount;
			if (isValidSlot)
			{
				bool isLoaded = LoadSlotData(slot);
				if (!isLoaded) return false;
			}
			else
			{
				currentSlot = null;
			}

			if (!vnManager)
				vnManager = FindObjectOfType<VNManager>();

			saveFileCoroutine = StartCoroutine(ApplySlotData());
			return true;
		}

		bool LoadSlotData(int slot)
		{
			if (!saveFileManager.HasSave(slot)) return false;

			currentSlot = saveFileManager.LoadSlot(slot);
			if (currentSlot == null || currentSlot.historyStates == null)
			{
				Debug.LogWarning($"Unable to repopulate history states because the loaded game save was invalid.");
				return false;
			}

			return true;
		}

		IEnumerator ApplySlotData()
		{
			// Wait for the dialogue flow controller to load the dialogue map and run the initialization scene
			while (!vnManager.Dialogue.FlowController.IsInitialized) yield return null;

			if (currentSlot != null && currentSlot.historyStates.Count > 0) // Have history restore progress
				yield return vnManager.History.ApplySaveFileHistory(currentSlot.historyStates);
			else // Start from the first scene if no save data is available
				vnManager.Dialogue.StartDialogue(null, -1);

			saveFileCoroutine = null;
		}
	}
}
