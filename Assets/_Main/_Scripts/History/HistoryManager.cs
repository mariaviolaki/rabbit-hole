using Dialogue;
using System.Collections.Generic;
using UnityEngine;

namespace History
{
	public class HistoryManager : MonoBehaviour
	{
		[SerializeField] FontBankSO fontBank;
		[SerializeField] DialogueSystem dialogueSystem;

		// TODO deserialize and manage dynamically
		[SerializeField] List<HistoryState> historyStates = new();

		public HistoryState Capture()
		{
			return new HistoryState(dialogueSystem);
		}

		public void Load(HistoryState historyState)
		{
			historyState.Apply(dialogueSystem, fontBank);
		}
	}
}
