using Dialogue;
using IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace History
{
	public class HistoryManager : MonoBehaviour
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] FontBankSO fontBank;
		[SerializeField] GameManager gameManager;
		[SerializeField] DialogueManager dialogueManager;

		const int MaxHistoryStates = 100;
		readonly LinkedList<HistoryState> historyStates = new();
		LinkedListNode<HistoryState> currentStateNode;
		int historyStateIndex = -1;
		bool isUpdatingHistory = false;
		HistoryAction lastHistoryAction = HistoryAction.None;

		public LinkedListNode<HistoryState> HistoryNode => currentStateNode;
		public HistoryAction LastAction => lastHistoryAction;
		public int CurrentDialogueLineId => currentStateNode == null ? 0 : currentStateNode.Value.Dialogue.DialogueLineId;
		public bool IsViewingHistory => currentStateNode != null && currentStateNode != historyStates.Last;
		public bool IsUpdatingHistory => isUpdatingHistory;
		
		void Awake()
		{
			inputManager.OnBack += GoBack;
		}

		void OnDestroy()
		{
			inputManager.OnBack -= GoBack;
		}

		public LinkedListNode<HistoryState> GetLastNode()
		{
			if (historyStates.Count <= 1) return null;
			else return currentStateNode?.Previous;
		}

		public int GetLastNodeIndex()
		{
			if (historyStates.Count <= 1) return -1;
			else return historyStateIndex;
		}

		public int GetHistoryStateCount()
		{
			if (historyStates.Count <= 1) return 0;
			else return historyStates.Count - historyStateIndex - 1;
		}

		public List<HistoryState> GetSaveFileHistoryStates()
		{
			if (currentStateNode == null || currentStateNode == historyStates.Last) return historyStates.ToList();

			// If the player is navigating back to history find the actual states shown at this moment
			List<HistoryState> navigationStates = new List<HistoryState>(historyStates.Count);
			for (LinkedListNode<HistoryState> node = historyStates.First; node != null; node = node.Next)
			{
				navigationStates.Add(node.Value);
				if (node == currentStateNode) break;
			}

			return navigationStates;
		}

		// Called when reading new dialogue
		public void Capture()
		{
			if (isUpdatingHistory || (currentStateNode != null && currentStateNode != historyStates.Last)) return;
			isUpdatingHistory = true;

			// Capture the current state of the visual novel
			historyStates.AddLast(new HistoryState(dialogueManager, gameManager.DialogueBank));
			currentStateNode = historyStates.Last;
			historyStateIndex = 0;

			// Don't allow the history to grow indefinitely
			if (historyStates.Count > MaxHistoryStates)
				historyStates.RemoveFirst();

			lastHistoryAction = HistoryAction.Capture;
			isUpdatingHistory = false;
		}

		void GoBack()
		{
			if (isUpdatingHistory || historyStates.Count <= 1 || currentStateNode.Previous == null) return;
			isUpdatingHistory = true;

			currentStateNode = currentStateNode.Previous;
			historyStateIndex++;

			StartCoroutine(Load(currentStateNode.Value));
		}

		public IEnumerator ApplySaveFileHistory(List<HistoryState> historyStatesArray)
		{
			if (historyStatesArray == null || historyStatesArray.Count == 0) yield break;

			while (isUpdatingHistory) yield return null;
			isUpdatingHistory = true;

			Repopulate(historyStatesArray);
			currentStateNode = historyStates.Last;
			historyStateIndex = 0;

			lastHistoryAction = HistoryAction.SaveApply;
			yield return currentStateNode.Value.Apply(dialogueManager, fontBank);
		}

		public IEnumerator ApplyLogPanelHistory(LinkedListNode<HistoryState> logPanelNode)
		{
			if (logPanelNode == null) yield break;
			while (isUpdatingHistory) yield return null;
			if (logPanelNode == historyStates.Last) yield break;

			isUpdatingHistory = true;

			currentStateNode = logPanelNode;
			lastHistoryAction = HistoryAction.LogApply;
			yield return currentStateNode.Value.Apply(dialogueManager, fontBank);
		}

		public IEnumerator ApplyNavigationHistory()
		{
			if (currentStateNode == null) yield break;
			while (isUpdatingHistory) yield return null;
			if (currentStateNode == historyStates.Last) yield break;

			isUpdatingHistory = true;

			lastHistoryAction = HistoryAction.NavigationApply;
			yield return currentStateNode.Value.Apply(dialogueManager, fontBank);
		}

		public void ResetHistoryProgress(HistoryState historyState)
		{
			historyState.Dialogue.RestoreDialogueProgress(dialogueManager.Reader.Stack);

			DeleteHistoryAfterCurrentNode();

			if (lastHistoryAction == HistoryAction.NavigationApply)
			{
				DialogueBlock lastBlock = dialogueManager.Reader.Stack.GetBlock();
				dialogueManager.Reader.Stack.Proceed(lastBlock);
			}
			else if (lastHistoryAction == HistoryAction.LogApply || lastHistoryAction == HistoryAction.SaveApply)
			{
				historyStates.RemoveLast();
				currentStateNode = historyStates.Last;
			}

			isUpdatingHistory = false;
		}

		IEnumerator Load(HistoryState historyState)
		{
			dialogueManager.SetReadMode(DialogueReadMode.Forward);
			dialogueManager.Reader.IsWaitingToAdvance = true;
			yield return historyState.Load(dialogueManager, fontBank, false);

			lastHistoryAction = HistoryAction.Load;
			isUpdatingHistory = false;
		}

		void Repopulate(List<HistoryState> historyStatesArray)
		{
			historyStates.Clear();
			currentStateNode = null;
			historyStateIndex = -1;

			foreach (HistoryState historyState in historyStatesArray)
			{
				historyStates.AddLast(historyState);
			}
		}

		void DeleteHistoryAfterCurrentNode()
		{
			if (currentStateNode == null) return;

			LinkedListNode<HistoryState> lastNode = historyStates.Last;

			while (lastNode != null && lastNode != currentStateNode)
			{
				LinkedListNode<HistoryState> nodeToRemove = lastNode;
				lastNode = lastNode.Previous;
				historyStates.Remove(nodeToRemove);
			}

			currentStateNode = historyStates.Last;
			historyStateIndex = 0;
		}
	}
}
