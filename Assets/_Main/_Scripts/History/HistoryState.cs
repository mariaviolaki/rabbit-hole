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

		public HistoryState(DialogueSystem dialogueSystem)
		{
			dialogueData = new(dialogueSystem.Reader.Stack, dialogueSystem.UI.Dialogue);
			audioData = new(dialogueSystem.Audio);
			visualData = new(dialogueSystem.Visuals);
			characterData = new(dialogueSystem.Characters);
			variableData = new(dialogueSystem.VariableManager, dialogueSystem.TagManager);
		}

		// Safely traverses older state
		public void Load(DialogueSystem dialogueSystem, FontBankSO fontBank)
		{
			dialogueData.Load(dialogueSystem.UI.Dialogue, dialogueSystem.Reader, dialogueSystem.Options, fontBank);
			audioData.Load(dialogueSystem.Audio, dialogueSystem.Options);
			visualData.Load(dialogueSystem.Visuals, dialogueSystem.Options);
			characterData.Load(dialogueSystem.Characters, dialogueSystem.Options);
		}

		// Resets progress to an older state
		public IEnumerator Apply(DialogueSystem dialogueSystem, FontBankSO fontBank)
		{
			Load(dialogueSystem, fontBank);
			variableData.Apply(dialogueSystem.VariableManager, dialogueSystem.TagManager);
			yield return dialogueData.Apply(dialogueSystem);
		}
	}
}
