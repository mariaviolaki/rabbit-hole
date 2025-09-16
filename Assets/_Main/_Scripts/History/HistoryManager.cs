using Dialogue;
using IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VN;

namespace History
{
	public class HistoryManager : MonoBehaviour
	{
		[SerializeField] VNOptionsSO vnOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] FontBankSO fontBank;
		[SerializeField] VNManager vnManager;
		[SerializeField] DialogueManager dialogueManager;

		const int MaxHistoryStates = 100;

		DialogueFlowController flowController;
		readonly LinkedList<HistoryState> historyStates = new();

		bool isLoadingHistory = false;
		bool isCapturePending = false;
		LinkedListNode<HistoryState> pendingLoadNode = null;

		public List<HistoryState> HistoryStates => historyStates.ToList();
		public LinkedListNode<HistoryState> HistoryNode => historyStates.Last;
		public HistoryState HistoryState => historyStates.Last?.Value;
		public int HistoryStateCount => historyStates.Count - 1;
		public bool IsUpdatingHistory => isLoadingHistory || isCapturePending || pendingLoadNode != null;

		void Awake()
		{
			inputManager.OnDialogueBack += GoBack;
		}

		void Start()
		{
			flowController = dialogueManager.FlowController;
		}

		void OnDestroy()
		{
			inputManager.OnDialogueBack -= GoBack;
		}

		void Update()
		{
			ProcessHistoryRequests();
		}

		// Called when reading new dialogue
		public void Capture()
		{
			isCapturePending = true;
		}

		public void Load(LinkedListNode<HistoryState> historyNode)
		{
			if (IsUpdatingHistory) return;
			pendingLoadNode = historyNode;
		}

		void GoBack()
		{
			if (IsUpdatingHistory) return;
			pendingLoadNode = historyStates.Last?.Previous;
		}

		IEnumerator ProcessLoad(LinkedListNode<HistoryState> historyNode)
		{
			if (historyNode == null)
			{
				pendingLoadNode = null;
				yield break;
			}

			isLoadingHistory = true;

			// Complete the current node quickly to move to the previous one
			while (flowController.CurrentNode != null)
			{
				flowController.StopDialogue();
				yield return null;
			}

			yield return historyNode.Value.Load(vnManager, dialogueManager, vnOptions);
			DeleteHistoryAfterCurrentNode(historyNode);

			flowController.ContinueDialogue();
			flowController.DialogueText.UpdateTextBuildMode(DialogueReadMode.Skip);

			while (!isCapturePending) yield return null;
			while (isCapturePending) yield return null;

			pendingLoadNode = null;
			isLoadingHistory = false;
		}

		public IEnumerator ApplySaveFileHistory(List<HistoryState> historyStatesArray)
		{
			if (historyStatesArray == null || historyStatesArray.Count == 0) yield break;

			while (IsUpdatingHistory) yield return null;
			isLoadingHistory = true;

			Repopulate(historyStatesArray);
			yield return historyStates.Last.Value.Load(vnManager, dialogueManager, vnOptions);
			DeleteHistoryAfterCurrentNode(historyStates.Last);

			flowController.ContinueDialogue();
			isLoadingHistory = false;
		}

		void ProcessHistoryRequests()
		{
			if (isCapturePending)
				ProcessCapture();
			else if (!isLoadingHistory && pendingLoadNode != null)
				StartCoroutine(ProcessLoad(pendingLoadNode));
		}

		void ProcessCapture()
		{
			// Capture the current state of the visual novel (for appended text, don't capture the node multiple times)
			HistoryState newState = new HistoryState(vnManager, dialogueManager);
			if (historyStates.Count == 0 || newState.Dialogue.DialogueNodeId != historyStates.Last.Value.Dialogue.DialogueNodeId)
			{
				historyStates.AddLast(newState);

				// Don't allow the history to grow indefinitely
				if (historyStates.Count > MaxHistoryStates)
					historyStates.RemoveFirst();
			}

			isCapturePending = false;
		}

		void Repopulate(List<HistoryState> historyStatesArray)
		{
			historyStates.Clear();

			foreach (HistoryState historyState in historyStatesArray)
			{
				historyStates.AddLast(historyState);
			}
		}

		void DeleteHistoryAfterCurrentNode(LinkedListNode<HistoryState> historyNode)
		{
			if (historyNode == null) return;

			LinkedListNode<HistoryState> currentNode = historyStates.Last;

			while (currentNode != null)
			{
				LinkedListNode<HistoryState> nodeToRemove = currentNode;
				currentNode = currentNode.Previous;
				historyStates.Remove(nodeToRemove);

				if (nodeToRemove == historyNode) break;
			}
		}
	}
}
