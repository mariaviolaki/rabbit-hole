using Dialogue;
using UnityEngine;

namespace History
{
	[System.Serializable]
	public class HistoryState
	{
		[SerializeField] HistoryDialogueData dialogueData;
		[SerializeField] HistoryAudioData audioData;
		[SerializeField] HistoryVisualData visualData;
		[SerializeField] HistoryCharacterData characterData;

		public HistoryState(DialogueSystem dialogueSystem)
		{
			dialogueData = new(dialogueSystem.UI.Dialogue);
			audioData = new(dialogueSystem.Audio);
			visualData = new(dialogueSystem.Visuals);
			characterData = new(dialogueSystem.Characters);
		}

		public void Apply(DialogueSystem dialogueSystem, FontBankSO fontBank)
		{
			dialogueData.Apply(dialogueSystem.UI.Dialogue, dialogueSystem.Reader, dialogueSystem.Options, fontBank);
			audioData.Apply(dialogueSystem.Audio, dialogueSystem.Options);
			visualData.Apply(dialogueSystem.Visuals, dialogueSystem.Options);
			characterData.Apply(dialogueSystem.Characters, dialogueSystem.Options);
		}
	}
}
