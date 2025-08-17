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
		[SerializeField] GameOptionsSO gameOptions;
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] FontBankSO fontBank;
		[SerializeField] GameManager gameManager;
		[SerializeField] DialogueManager dialogueManager;

		const int MaxHistoryStates = 100;

		DialogueFlowController flowController;
		readonly LinkedList<HistoryState> historyStates = new();
		bool isUpdatingHistory = false;

		public List<HistoryState> HistoryStates => historyStates.ToList();
		public LinkedListNode<HistoryState> HistoryNode => historyStates.Last;
		public HistoryState HistoryState => historyStates.Last?.Value;
		public bool IsUpdatingHistory => isUpdatingHistory;
		
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

		public int GetHistoryStateCount()
		{
			return historyStates.Count - 1;
		}

		// Called when reading new dialogue
		public void Capture()
		{
			if (isUpdatingHistory) return;
			isUpdatingHistory = true;

			// Capture the current state of the visual novel
			historyStates.AddLast(new HistoryState(dialogueManager));

			// Don't allow the history to grow indefinitely
			if (historyStates.Count > MaxHistoryStates)
				historyStates.RemoveFirst();

			isUpdatingHistory = false;
		}

		void GoBack()
		{
			StartCoroutine(Load(historyStates.Last?.Previous));
		}

		public IEnumerator Load(LinkedListNode<HistoryState> historyNode)
		{
			if (IsUpdatingHistory || historyNode == null) yield break;

			isUpdatingHistory = true;

			// Complete the current node quickly to move to the previous one
			while (flowController.CurrentNode != null)
			{
				flowController.StopDialogue();
				yield return null;
			}

			yield return historyNode.Value.Load(dialogueManager, gameOptions);
			DeleteHistoryAfterCurrentNode(historyNode);

			isUpdatingHistory = false;
			flowController.StartDialogue();
		}

		public IEnumerator ApplySaveFileHistory(List<HistoryState> historyStatesArray)
		{
			if (historyStatesArray == null || historyStatesArray.Count == 0) yield break;

			while (isUpdatingHistory) yield return null;
			isUpdatingHistory = true;

			Repopulate(historyStatesArray);
			yield return historyStates.Last.Value.Load(dialogueManager, gameOptions);
			DeleteHistoryAfterCurrentNode(historyStates.Last);

			isUpdatingHistory = false;
			flowController.StartDialogue();
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
