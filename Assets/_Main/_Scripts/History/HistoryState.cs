using Dialogue;
using System.Collections;
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
		[SerializeField] HistoryVariableData variableData;

		public HistoryDialogueData Dialogue => dialogueData;
		public HistoryAudioData Audio => audioData;
		public HistoryVisualData Visuals => visualData;
		public HistoryCharacterData Characters => characterData;
		public HistoryVariableData Variables => variableData;

		public HistoryState(DialogueManager dialogueManager, DialogueLineBank dialogueLineBank)
		{
			dialogueData = new(dialogueManager.Reader.Stack, dialogueLineBank, dialogueManager.UI.Dialogue);
			audioData = new(dialogueManager.Audio);
			visualData = new(dialogueManager.Visuals);
			characterData = new(dialogueManager.Characters);
			variableData = new(dialogueManager.VariableManager, dialogueManager.TagManager);
		}

		// Safely traverses older state
		public IEnumerator Load(DialogueManager dialogueManager, FontBankSO fontBank, bool isApplyingHistory)
		{
			audioData.Load(dialogueManager.Audio, dialogueManager.Options);
			yield return visualData.Load(dialogueManager.Visuals, dialogueManager.Options);
			yield return characterData.Load(dialogueManager.Characters, dialogueManager.Options);

			if (isApplyingHistory) yield break;

			yield return dialogueData.Load(dialogueManager.UI.Dialogue, dialogueManager.Reader, dialogueManager.Options, fontBank);
		}

		// Resets progress to an older state
		public IEnumerator Apply(DialogueManager dialogueManager, FontBankSO fontBank)
		{
			yield return Load(dialogueManager, fontBank, true);
			variableData.Apply(dialogueManager.VariableManager, dialogueManager.TagManager);
			dialogueData.Apply(this, dialogueManager);
		}
	}
}
