using Dialogue;
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
		LinkedList<HistoryState> historyStates = new();
		LinkedListNode<HistoryState> currentStateNode;

		public bool IsViewingHistory => currentStateNode != null;

		void Awake()
		{
			inputManager.OnBack += GoBack;
		}

		void OnDestroy()
		{
			inputManager.OnBack -= GoBack;
		}

		// Called when reading new dialogue
		public void Capture()
		{
			if (IsViewingHistory) return;			

			// Capture the current state of the visual novel
			historyStates.AddLast(new HistoryState(dialogueSystem));

			// Don't allow the history to grow indefinitely
			if (historyStates.Count > MaxHistoryStates)
				historyStates.RemoveFirst();
		}

		public void GoBack()
		{
			if (historyStates.Count == 0) return;

			if (!IsViewingHistory)
				currentStateNode = historyStates.Last;
			else if (currentStateNode.Previous != null)
				currentStateNode = currentStateNode.Previous;
			else return; // No previous state to go back to

			Load(currentStateNode.Value);

			dialogueSystem.ResetReadMode();
			dialogueSystem.Reader.PauseDialogue();
		}

		public IEnumerator GoForward()
		{
			if (!IsViewingHistory) yield break;

			// Apply the current history state
			yield return Apply(currentStateNode.Value);
			DeleteMoreRecentHistory();

			// Move the dialogue forward after the stack has been updated
			DialogueBlock lastBlock = dialogueSystem.Reader.Stack.GetBlock();
			dialogueSystem.Reader.Stack.Proceed(lastBlock);
			dialogueSystem.ReadDialogue(DialogueReadMode.None);
		}

		void DeleteMoreRecentHistory()
		{
			LinkedListNode<HistoryState> lastNode = historyStates.Last;

			while (lastNode != null && lastNode != currentStateNode)
			{
				LinkedListNode<HistoryState> nodeToRemove = lastNode;
				lastNode = lastNode.Previous;
				historyStates.Remove(nodeToRemove);
			}

			currentStateNode = null;
		}

		void Load(HistoryState historyState)
		{
			historyState.Load(dialogueSystem, fontBank);
		}

		IEnumerator Apply(HistoryState historyState)
		{
			yield return historyState.Apply(dialogueSystem, fontBank);
		}
	}
}
