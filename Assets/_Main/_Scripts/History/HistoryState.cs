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

		public HistoryState(GameManager gameManager, DialogueManager dialogueManager)
		{
			dialogueData = new(dialogueManager.FlowController);
			audioData = new(dialogueManager.Audio);
			visualData = new(dialogueManager.Visuals);
			characterData = new(dialogueManager.Characters);
			variableData = new(gameManager.Variables);
		}

		// Resets progress to an older state
		public IEnumerator Load(GameManager gameManager, DialogueManager dialogueManager, GameOptionsSO gameOptions)
		{
			yield return audioData.Load(dialogueManager.Audio, gameOptions);
			yield return visualData.Load(dialogueManager.Visuals, gameOptions);
			characterData.Load(dialogueManager.Characters, gameOptions);
			yield return dialogueData.Load(dialogueManager, dialogueManager.FlowController);
			variableData.Load(gameManager.Variables);
		}
	}
}
