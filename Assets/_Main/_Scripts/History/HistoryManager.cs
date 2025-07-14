using Dialogue;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace History
{
	public class HistoryManager : MonoBehaviour
	{
		[SerializeField] InputManagerSO inputManager;
		[SerializeField] FontBankSO fontBank;
		[SerializeField] DialogueSystem dialogueSystem;

		const int MaxHistoryStates = 100;
		readonly LinkedList<HistoryState> historyStates = new();
		LinkedListNode<HistoryState> currentStateNode;
		LinkedListNode<HistoryState> logStateNode;
		int historyStateIndex = -1;
		bool isUpdatingHistory = false;

		public LinkedListNode<HistoryState> HistoryNode => currentStateNode;
		public LinkedListNode<HistoryState> LogNode => logStateNode;
		public bool IsViewingHistory => currentStateNode != null || logStateNode != null;

		public bool IsUpdatingHistory { get { return isUpdatingHistory; } set { isUpdatingHistory = value; } }

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
			if (historyStates.Count == 0) return null;
			else if (currentStateNode == null && !dialogueSystem.Reader.Stack.IsEmpty) return historyStates.Last;
			else if (currentStateNode == null) return historyStates.Last.Previous;
			else return currentStateNode.Previous;
		}

		public int GetLastNodeIndex()
		{
			if (historyStates.Count == 0 || (dialogueSystem.Reader.Stack.IsEmpty && historyStates.Count == 1)) return -1;
			else if (currentStateNode == null && dialogueSystem.Reader.Stack.IsEmpty) return 1;
			else if (currentStateNode == null) return 0;
			else return historyStateIndex;
		}

		public int GetHistoryStateCount()
		{
			if (dialogueSystem.Reader.Stack.IsEmpty) return Mathf.Max(historyStates.Count - 1, 0);
			else if(currentStateNode == null) return historyStates.Count; 
			else return historyStates.Count - historyStateIndex - 1;
		}

		// Called when reading new dialogue
		public void Capture()
		{
			if (currentStateNode != null || isUpdatingHistory) return;
			isUpdatingHistory = true;

			// Capture the current state of the visual novel
			historyStates.AddLast(new HistoryState(dialogueSystem));

			// Don't allow the history to grow indefinitely
			if (historyStates.Count > MaxHistoryStates)
				historyStates.RemoveFirst();

			isUpdatingHistory = false;
		}

		void GoBack()
		{
			if (historyStates.Count == 0 || isUpdatingHistory) return;
			isUpdatingHistory = true;

			if (currentStateNode == null)
			{
				if (!dialogueSystem.Reader.Stack.IsEmpty)
				{
					// Get the most recently captured history state
					currentStateNode = historyStates.Last;
					historyStateIndex = 0;
				}
				else if (historyStates.Last.Previous != null)
				{
					// If we have reached the end of the current dialogue, the last state is already being shown
					currentStateNode = historyStates.Last.Previous;
					historyStateIndex = 1;
				}
				else // No previous state to go back to
				{
					isUpdatingHistory = false;
					return;
				}
			}
			else if (currentStateNode.Previous != null)
			{
				// We are already traversing history, go further back
				currentStateNode = currentStateNode.Previous;
				historyStateIndex++;
			}
			else // No previous state to go back to
			{
				isUpdatingHistory = false;
				return;
			}
	
			StartCoroutine(Load(currentStateNode.Value));
		}

		public IEnumerator ApplyHistory(Action<bool> isApplied)
		{
			if (isUpdatingHistory || (currentStateNode == null && logStateNode == null))
			{
				logStateNode = null;
				isApplied(false);
				yield break;
			}

			isUpdatingHistory = true;	

			if (logStateNode != null)
			{
				yield return logStateNode.Value.Apply(dialogueSystem, fontBank);
				DeleteLogHistory();
			}
			else
			{
				yield return currentStateNode.Value.Apply(dialogueSystem, fontBank);
				DeleteNavigationHistory();
			}

			isApplied(true);
			isUpdatingHistory = false;
		}

		public void SetLogPanelRewindNode(LinkedListNode<HistoryState> node)
		{
			if (!inputManager.IsLogPanelOpen || node == null || IsUpdatingHistory) return;

			logStateNode = node;
		}

		IEnumerator Load(HistoryState historyState)
		{
			yield return historyState.Load(dialogueSystem, fontBank, false);

			dialogueSystem.ResetReadMode();
			dialogueSystem.Reader.WaitForInput();

			isUpdatingHistory = false;
		}

		void DeleteNavigationHistory()
		{
			if (currentStateNode == null) return;

			LinkedListNode<HistoryState> lastNode = historyStates.Last;

			// Preserve the current state because the dialogue is about to progress to the next one
			while (lastNode != null && lastNode != currentStateNode)
			{
				LinkedListNode<HistoryState> nodeToRemove = lastNode;
				lastNode = lastNode.Previous;
				historyStates.Remove(nodeToRemove);
			}

			currentStateNode = null;
			logStateNode = null;
			historyStateIndex = -1;
		}

		void DeleteLogHistory()
		{
			if (logStateNode == null) return;

			LinkedListNode<HistoryState> lastNode = historyStates.Last;

			// Delete the current state because the dialogue is about to recapture it
			while (lastNode != null)
			{
				LinkedListNode<HistoryState> nodeToRemove = lastNode;
				bool isLogNode = nodeToRemove == logStateNode;

				lastNode = lastNode.Previous;
				historyStates.Remove(nodeToRemove);

				if (isLogNode) break;
			}

			currentStateNode = null;
			logStateNode = null;
			historyStateIndex = -1;
		}
	}
}
