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

		public HistoryState(DialogueSystem dialogueSystem)
		{
			dialogueData = new(dialogueSystem.Reader.Stack, dialogueSystem.UI.Dialogue);
			audioData = new(dialogueSystem.Audio);
			visualData = new(dialogueSystem.Visuals);
			characterData = new(dialogueSystem.Characters);
			variableData = new(dialogueSystem.VariableManager, dialogueSystem.TagManager);
		}

		// Safely traverses older state
		public IEnumerator Load(DialogueSystem dialogueSystem, FontBankSO fontBank, bool isApplyingHistory)
		{
			audioData.Load(dialogueSystem.Audio, dialogueSystem.Options);
			yield return visualData.Load(dialogueSystem.Visuals, dialogueSystem.Options);
			yield return characterData.Load(dialogueSystem.Characters, dialogueSystem.Options);

			if (isApplyingHistory) yield break;

			yield return dialogueData.Load(dialogueSystem.UI.Dialogue, dialogueSystem.Reader, dialogueSystem.Options, fontBank);
		}

		// Resets progress to an older state
		public IEnumerator Apply(DialogueSystem dialogueSystem, FontBankSO fontBank)
		{
			yield return Load(dialogueSystem, fontBank, true);
			variableData.Apply(dialogueSystem.VariableManager, dialogueSystem.TagManager);
			yield return dialogueData.Apply(dialogueSystem);
		}
	}
}
